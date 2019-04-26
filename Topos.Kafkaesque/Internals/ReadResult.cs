using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Topos.Internals
{
    class ReadResult : IEnumerable<string>
    {
        public static readonly ReadResult Empty = new ReadResult(new List<string>(), () => { }, 0, 0);

        readonly Action _completionAction;
        readonly List<string> _lines;

        internal ReadResult(List<string> lines, Action completionAction, int fileNumber, int bytePosition)
        {
            _completionAction = completionAction;
            _lines = lines ?? new List<string>();
            Position = (fileNumber << 32) | bytePosition;
        }

        public bool IsEmpty => !_lines.Any();

        public long Position { get; }

        public IEnumerator<string> GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Complete()
        {
            _completionAction?.Invoke();
        }
    }
}