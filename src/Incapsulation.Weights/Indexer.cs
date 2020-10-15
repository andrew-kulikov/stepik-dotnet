using System;

namespace Incapsulation.Weights
{
    public class Indexer
    {
        private readonly double[] _src;
        private readonly int _start;

        public Indexer(double[] src, int start, int length)
        {
            if (start < 0 || length < 0 || (start + length > src.Length && length != 0))
            {
                throw new ArgumentException("Constructor should throw ArgumentException, when range is invalid!\n");
            }

            _src = src;
            _start = start;
            Length = length;
        }

        public int Length { get; }

        public double this[int id]
        {
            get
            {
                if (id >= Length || id < 0) throw new IndexOutOfRangeException();

                return _src[_start + id];
            }
            set
            {
                if (id >= Length || id < 0) throw new IndexOutOfRangeException();

                _src[_start + id] = value;
            }
        }
    }
}