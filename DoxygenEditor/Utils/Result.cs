using System;

namespace TSP.DoxygenEditor.Utils
{
    public readonly struct Result<T>
    {
        public Exception Error { get; }
        public T Value { get; }
        public bool Success { get; }

        public Result(Exception error)
        {
            Error = error;
            Value = default;
            Success = false;
        }

        public Result(T value)
        {
            Error = null;
            Value = value;
            Success = true;
        }
    }
}
