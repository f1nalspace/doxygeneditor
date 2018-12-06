using DoxygenEditor.MVVM;
using DoxygenEditor.Parser;
using DoxygenEditor.Services;
using System;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DoxygenEditor.Utils;
using System.Diagnostics;
using System.Reflection;
using DoxygenEditor.Editor;

namespace DoxygenEditor.ViewModels
{
    public class MainViewModel : ViewModelBase, IFileActions
    {
        private readonly IEditor _editor;
        private readonly BackgroundWorker _parseWorker;
        private readonly IDoxygenParser _parser;
        private ParseState _parseState;

        public ParseState ParseState
        {
            get { return _parseState; }
        }

        public IFileHandler FileHandler { get; }
        public bool IsChanged { get { return FileHandler.IsChanged; } }
        public string FilePath { get { return FileHandler.FilePath; } }
        public string LastStatus
        {
            get { return GetProperty(() => LastStatus); }
            set { SetProperty(() => LastStatus, value); }
        }
        public string LastParsedState
        {
            get { return GetProperty(() => LastParsedState); }
            set { SetProperty(() => LastParsedState, value); }
        }
        public string LastLexedState
        {
            get { return GetProperty(() => LastLexedState); }
            set { SetProperty(() => LastLexedState, value); }
        }
        public string WindowTitle
        {
            get
            {
                var v = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                string result = $"Doxygen-Editor v{v.FileVersion}";
                if (!string.IsNullOrEmpty(FileHandler.FilePath))
                    result += " - " + Path.GetFileName(FileHandler.FilePath);
                else
                    result += " - Unnamed file";
                if (FileHandler.IsChanged) result += "*";
                return (result);
            }
        }

        public delegate void InsertEditorControlEventHandler(object sender, Control control);
        public event InsertEditorControlEventHandler InsertEditorControl;

        public delegate void UpdateTreeEventHandler(object sender, ParseState parseState);
        public event UpdateTreeEventHandler UpdateTree;

        public DelegateCommand<object> NewFileCommand { get; }
        public DelegateCommand<string> OpenFileCommand { get; }
        public DelegateCommand<object> SaveFileCommand { get; }
        public DelegateCommand<object> SaveFileAsCommand { get; }

        public DelegateCommand<object> UndoCommand { get; }
        public DelegateCommand<object> RedoCommand { get; }

        public DelegateCommand<object> CutCommand { get; }
        public DelegateCommand<object> CopyCommand { get; }
        public DelegateCommand<object> PasteCommand { get; }

        public DelegateCommand<object> SelectAllCommand { get; }

        public DelegateCommand<object> QuickSearchCommand { get; }
        public DelegateCommand<object> QuickReplaceCommand { get; }

        public DelegateCommand<int> GoToLineCommand { get; }
        public DelegateCommand<int> GoToPositionCommand { get; }

        public DelegateCommand<bool> ViewWhitespacesCommand { get; }

        public bool CanClose()
        {
            _editor.StopTimers();
            if (_parseWorker.IsBusy)
            {
                var msgService = IOCContainer.Default.Get<IMessageBoxService>();
                msgService.Show("The parser is still working, please wait until it completes.", "Parser not finished yet", MsgBoxButtons.OK, MsgBoxIcon.Asterisk);
                return(false);
            }
            if (!FileHandler.CloseConfirmation())
            {
                return (false);
            }
            return (true);
        }

        private void UpdateFileCommands()
        {
            NewFileCommand.RaiseCanExecuteChanged();
            OpenFileCommand.RaiseCanExecuteChanged();
            SaveFileCommand.RaiseCanExecuteChanged();
            SaveFileAsCommand.RaiseCanExecuteChanged();
        }
        private void UpdateUndoCommands()
        {
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }
        private void UpdateCutCommands()
        {
            CutCommand.RaiseCanExecuteChanged();
            CopyCommand.RaiseCanExecuteChanged();
            PasteCommand.RaiseCanExecuteChanged();
        }

        public MainViewModel()
        {
            FileHandler = new FileHandler(this, "Doxygen files|*.docs", ".docs");
            FileHandler.PropertyChanged += (s, e) =>
            {
                RaisePropertiesChanged(() => IsChanged, () => FilePath);
                if (ReflectionUtils.GetMemberName((MainViewModel v) => v.IsChanged).Equals(e.PropertyName))
                {
                    UpdateFileCommands();
                    UpdateUndoCommands();
                    UpdateCutCommands();
                }
            };
            // @TODO(final): Control should not instaniated here, not MVVM conform!
            _editor = new ScintillaEditor(FileHandler, this);
            _editor.DelayedTextChanged += (s, text) =>
            {
                if (_parseWorker.IsBusy)
                    return (false);
                _parseWorker.RunWorkerAsync(text);
                return (true);
            };
            _editor.StyleChanged += (s, d) =>
            {
                LastLexedState = $"Styled in {d.TotalMilliseconds,0:F3} ms";
            };
            _editor.FocusChanged += (s, e) =>
            {
                UpdateUndoCommands();
                UpdateCutCommands();
            };
            _parser = new DoxygenParser();
            _parseWorker = new BackgroundWorker();
            _parseWorker.RunWorkerCompleted += _parseWorker_RunWorkerCompleted;
            _parseWorker.DoWork += _parseWorker_DoWork;
            _parseState = null;

            NewFileCommand = new DelegateCommand<object>((e) => {
                FileHandler.New();
            });
            OpenFileCommand = new DelegateCommand<string>((filePath) => {
                FileHandler.Open(filePath);
            });
            SaveFileCommand = new DelegateCommand<object>((e) => {
                FileHandler.SaveWithConfirmation();
            }, (e) => FileHandler.IsChanged);
            SaveFileAsCommand = new DelegateCommand<object>((e) => {
                FileHandler.SaveAs();
            });

            GoToLineCommand = new DelegateCommand<int>((lineIndex) => {
                _editor.GoToLine(lineIndex);
            });
            GoToPositionCommand = new DelegateCommand<int>((position) => {
                _editor.GoToPosition(position);
            });

            UndoCommand = new DelegateCommand<object>((e) => {
                _editor.Undo();
                UpdateUndoCommands();
            }, (e) => _editor.CanUndo());
            RedoCommand = new DelegateCommand<object>((e) => {
                _editor.Redo();
                UpdateUndoCommands();
            }, (e) => _editor.CanRedo());

            CutCommand = new DelegateCommand<object>((e) => {
                _editor.Cut();
                UpdateCutCommands();
            }, (e) => _editor.CanCut());
            CopyCommand = new DelegateCommand<object>((e) => {
                _editor.Copy();
                UpdateCutCommands();
            }, (e) => _editor.CanCopy());
            PasteCommand = new DelegateCommand<object>((e) => {
                _editor.Paste();
                UpdateCutCommands();
            }, (e) => _editor.CanPaste());

            SelectAllCommand = new DelegateCommand<object>((e) => {
                _editor.SelectAll();
            });
            QuickSearchCommand = new DelegateCommand<object>((e) => {
                _editor.ShowQuickSearch();
            });
            QuickReplaceCommand = new DelegateCommand<object>((e) => {
                _editor.ShowQuickReplace();
            });

            ViewWhitespacesCommand = new DelegateCommand<bool>((e) => {
                _editor.SetShowWhitespaces(e);
            });
        }

        public override void ViewLoaded(object view) {
            FileHandler.New();
            InsertEditorControl?.Invoke(this, _editor.GetControl());
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                FileHandler.Open(args[1]);
        }

        private void _parseWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LastParsedState = "Parsing text...";
            string text = (string)e.Argument;
            ParseState state = _parser.Parse(text);
            e.Result = state;
        }

        private void _parseWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _parseState = e.Result as ParseState;
            UpdateTree?.Invoke(this, _parseState);
            if (e.Error == null)
                LastParsedState = $"Successfully parsed text in {_parseState.Duration.TotalMilliseconds,0:F3} ms";
            else
                LastParsedState = $"Failed parsing text: {e.Error}";
        }

        public void FileNew()
        {
            _editor.SetText(string.Empty);
            LastStatus = "New file";
        }
        public void FileLoad(string filePath)
        {
            string filename = Path.GetFileName(filePath);
            LastStatus = $"Loading file '{filename}'";
            string text = File.ReadAllText(filePath);
            _editor.SetText(text);
            LastStatus = $"Loaded file '{filename}'";
        }
        public void FileSave(string filePath)
        {
            LastStatus = $"Saving file '{Path.GetFileName(filePath)}'";
            string text = _editor.GetText();
            Encoding encoding = Encoding.UTF8;
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    encoding = reader.CurrentEncoding;
                }
            }
            using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
            {
                writer.Write(text);
                writer.Flush();
            }
            LastStatus = $"Saved file '{Path.GetFileName(filePath)}'";
        }
    }
}
