using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Streams.Compression
{
    public class CustomCompressionStream : Stream
    {
        private Stream baseStream;
        private bool read;
        private List<byte> curChunk = new List<byte>();

        public CustomCompressionStream(Stream baseStream, bool read)
        {
            this.read = read;
            this.baseStream = baseStream;
        }

        public override bool CanRead => read;
        public override bool CanSeek => false;
        public override bool CanWrite => !read;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (baseStream.Length % 2 != 0) throw new InvalidOperationException();

            var got = 0;

            while (got < count)
            {
                var cur = ReadCurrentChunk(buffer, offset + got, count);
                got += cur;

                if (cur == 0 || (baseStream.CanRead && baseStream.Position == baseStream.Length)) break;
            }

            return got;
        }

        private int ReadCurrentChunk(byte[] buffer, int offset, int count)
        {
            var chunk = new List<byte>();

            while (curChunk.Count < count)
            {
                var buff = new byte[count];
                var cnt = baseStream.Read(buff, 0, count);
                if (cnt == 0) break;

                chunk.AddRange(buff.Take(cnt));

                if (chunk.Count % 2 == 0)
                {
                    curChunk.AddRange(Decode(chunk.ToArray()));
                    chunk = new List<byte>();
                }
            }

            var result = curChunk.Take(count).ToList();
            curChunk = curChunk.Skip(count).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                buffer[i + offset] = result[i];
            }

            return result.Count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var encoded = Encode(buffer.Skip(offset).Take(count).ToArray()).ToArray();
            baseStream.Write(encoded, 0, encoded.Length);
        }

        private IEnumerable<byte> Encode(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length;)
            {
                byte cnt = 0;

                while (i + cnt < buffer.Length && cnt < byte.MaxValue && buffer[i] == buffer[i + cnt]) cnt++;

                yield return cnt;
                yield return buffer[i];

                i += cnt;
            }
        }

        private IEnumerable<byte> Decode(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length - 1; i += 2)
            {
                var cnt = buffer[i];
                var num = buffer[i + 1];

                for (int j = 0; j < cnt; j++) yield return num;
            }
        }
    }
}