using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Topos.FileSystem
{
    class ReadResult : IEnumerable<string>
    {
        public static readonly ReadResult Empty = new ReadResult(new List<string>(), () => { });

        readonly Action _completionAction;
        readonly List<string> _lines;

        internal ReadResult(List<string> lines, Action completionAction)
        {
            _completionAction = completionAction;
            _lines = lines ?? new List<string>();
        }

        public bool IsEmpty => !_lines.Any();

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
    }}