using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TSP.DoxygenEditor.Languages.Cpp;
using TSP.DoxygenEditor.Symbols;
using TSP.DoxygenEditor.TextAnalysis;

namespace TSP.DoxygenEditor.Includes
{
    public class SourceIncludesLoader
    {
        enum State : int
        {
            Stopped,
            Running,
            Paused,
            Complete,
        }

        private volatile State _state;

        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        private readonly ConcurrentDictionary<Task, Task<List<SymbolTable>>> _tasks = new ConcurrentDictionary<Task, Task<List<SymbolTable>>>();
        private readonly ConcurrentBag<SymbolTable> _tables = new ConcurrentBag<SymbolTable>();

        public bool IsPaused => _state == State.Paused;
        public bool IsStopped => _state == State.Stopped;
        public bool IsRunning => _state == State.Running;
        public bool IsComplete => _state == State.Complete;

        private readonly int _totalFileCount;
        private readonly int _maxTaskCount;
        private volatile int _progressFileCount = 0;
        private volatile int _runningTaskCount = 0;

        public delegate void IsCompletedEventHandler(object sender, IEnumerable<SymbolTable> tables);
        public event IsCompletedEventHandler IsCompleted;

        public delegate void ProgressChangedEventHandler(object sender, int parsedFileCount, int totalFileCount);
        public event ProgressChangedEventHandler ProgressChanged;

        class IncludeFileId : ISymbolTableId
        {
            private readonly string _filename;
            public object SymbolTableId => _filename;
            public IncludeFileId(string filename)
            {
                _filename = filename;
            }
        }

        public SourceIncludesLoader(IEnumerable<string> files, int maxTaskCount)
        {
            _state = State.Stopped;
            _totalFileCount = files.Count();
            _progressFileCount = 0;
            _maxTaskCount = maxTaskCount;
            foreach (var file in files)
                _queue.Enqueue(file);
        }

        public void Start()
        {
            Debug.Assert(_state == State.Stopped || _state == State.Paused);
            Debug.Assert(_maxTaskCount > 0);
            Debug.Assert(_tasks.Count == 0);
            _state = State.Running;
            _runningTaskCount = 0;
            _progressFileCount = 0;
            ProgressChanged?.Invoke(this, 0, _totalFileCount);
            for (int i = 0; i < _maxTaskCount; ++i)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    Interlocked.Increment(ref _runningTaskCount);
                    List<SymbolTable> tables = new List<SymbolTable>();
                    while (!_queue.IsEmpty && _state == State.Running)
                    {
                        string filePath;
                        if (_queue.TryDequeue(out filePath))
                        {
                            SymbolTable table = new SymbolTable(new IncludeFileId(filePath));
                            try
                            {
                                string source = File.ReadAllText(filePath);
                                List<CppToken> _tokens = new List<CppToken>();
                                using (CppLexer lexer = new CppLexer(source, new TextPosition(), source.Length, Languages.LanguageKind.Cpp))
                                {
                                    foreach (var err in lexer.LexErrors)
                                        Debug.WriteLine($"Lex error[{filePath}]: {err.Message}");
                                    _tokens.AddRange(lexer.Tokenize());
                                }
                                using (CppParser parser = new CppParser(table.Id, new CppParser.CppConfiguration()))
                                {
                                    parser.ParseTokens(_tokens);
                                    foreach (var err in parser.ParseErrors)
                                        Debug.WriteLine($"Parse error[{filePath}]: {err.Message}");
                                    table.AddTable(parser.SymbolTable);
                                }
                                table.IsValid = true;
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine($"Failed parsing include file '{filePath}': {e.Message}");
                            }
                            tables.Add(table);
                            Interlocked.Increment(ref _progressFileCount);
                            ProgressChanged?.Invoke(this, _progressFileCount, _totalFileCount);
                        }
                        Thread.Yield();
                    }
                    return (tables);
                });
                task.ContinueWith((t) =>
                {
                    foreach (var table in t.Result)
                        _tables.Add(table);
                    while (_tasks.ContainsKey(t) && !_tasks.TryRemove(t, out _))
                        Thread.Yield();
                    if (Interlocked.Decrement(ref _runningTaskCount) == 0)
                    {
                        // All tasks are done, this does not nessecarly mean that everything is finished
                        if (_queue.IsEmpty)
                        {
                            _state = State.Complete;
                            IsCompleted?.Invoke(this, _tables);
                        }
                        else
                            _state = State.Paused;
                    }
                });
                _tasks.AddOrUpdate(task, task, (key, existingVal) =>
                {
                    if (task != existingVal)
                        throw new ArgumentException("Duplicate task are not allowed");
                    return (existingVal);
                });
            }
        }

        public void Pause()
        {
            if (_state == State.Running)
                _state = State.Paused;
            else
                throw new Exception($"Cannot pause include loader in state {_state}");
        }

        public void Stop()
        {
            if (_state == State.Running || _state == State.Paused)
            {
                _state = State.Stopped;
                _tasks.Clear();
            }
            else
                throw new Exception($"Cannot stop include loader in state {_state}");
        }
    }
}
