using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Streams.Resources
{
    internal class TestStream : Stream
    {
        private readonly byte[] data;
        private int pointer;
        private readonly bool infinityMode;

        public TestStream(byte[] data)
        {
            this.data = data;
            infinityMode = false;
        }

        public TestStream(IEnumerable<string> keysAndValues, byte[] separator = null, bool infinityMode = false)
        {
            this.infinityMode = infinityMode;
            if (separator == null)
                separator = new byte[] { 0, 1 };

            var bytes = new List<byte>();
            foreach (var e in keysAndValues)
            {
                foreach (var b in Encoding.ASCII.GetBytes(e))
                {
                    if (b == 0) bytes.Add(0);
                    bytes.Add(b);
                }

                foreach (var bt in separator)
                {
                    bytes.Add(bt);
                }
            }
            data = bytes.ToArray();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count != Constants.BufferSize)
                throw new InvalidOperationException("Все чтения из нижележащего стрима должны быть с count=" + Constants.BufferSize);

            if (infinityMode && pointer >= data.Length)
                return RandomRead(buffer, offset, count);

            count = Math.Min(count, data.Length - pointer);
            for (var i = 0; i < count; i++)
                buffer[i + offset] = data[i + pointer];
            pointer += count;
            return count;
        }

        private int RandomRead(byte[] buffer, int offset, int count)
        {
            var random = new Random();
            var i = 0;
            if (count > 2 && buffer.Length > 2)
            {
                buffer[offset] = 0;
                buffer[offset + 1] = 1;
                i += 2;
            }

            for (; i < count; i++)
                buffer[i + offset] = (byte)random.Next(255);
            return count;
        }

        public override bool CanRead => true;

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
