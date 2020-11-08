using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Streams.Resources
{
    public class ResourceReaderStream : Stream
    {
        private const int Size = 1024;

        private readonly string _key;
        private Stream _underlyingStream;
        private List<byte> _found;
        private bool _valueEndFound;
        private string _foundKey = string.Empty;

        public ResourceReaderStream(Stream stream, string key)
        {
            _key = key;
            _underlyingStream = stream;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
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
            if (_found == null) SeekValue();

            if (_found == null) throw new EndOfStreamException();

            if (!_found.Any()) return 0;

            CheckEndOfStream();
            if (count > _found.Count) ReadNext();
            CheckEndOfStream();

            int i = 0, j = 0;

            while (i < count)
            {
                if (j == _found.Count) break;
                if (j < _found.Count - 1 && _found[j] == 0 && _found[j + 1] == 0) j++;

                buffer[i + offset] = _found[j];
                i++; j++;
            }

            _found = _found.Skip(j).ToList();

            return i;
        }

        private void CheckEndOfStream()
        {
            if (_found.Count >= 2 && _found[_found.Count - 2] == 0 && _found[_found.Count - 1] == 1)
            {
                _found = _found.Take(_found.Count - 2).ToList();
                _valueEndFound = true;
            }
        }

        private void ReadNext()
        {
            if (_valueEndFound) return;

            var buff = new byte[Size];
            var read = _underlyingStream.Read(buff, 0, Size);

            if (read == 0) return;

            var parts = Split(buff).ToList();
            _found.AddRange(parts[0]);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private void SeekValue()
        {
            // while not end of stream read next section key, compare with required key and skip value if read key is not equal to required key
            while (_found == null)
            {
                var buff = new byte[Size];
                var read = _underlyingStream.Read(buff, 0, Size);

                if (read == 0) break;

                var parts = Split(buff.Take(read).ToArray()).ToList();
                for (int i = 0; i < parts.Count; i++)
                {
                    var partWithoutEnd = GetKeyWithoutEnd(parts[i].ToList());

                    _foundKey += Encoding.ASCII.GetString(partWithoutEnd);

                    if (_foundKey == _key)
                    {
                        if (i < parts.Count - 1)
                        {
                            if (parts[i + 1].Any(x => x != 0)) _found = parts[i + 1].ToList();
                        }
                        else if (parts.Count != 1) _found = new List<byte>();

                        if (_found != null && read < Size && _found[_found.Count - 2] != 0 && _found[_found.Count - 1] != 1)
                            _found = null;

                        break;
                    }
                    
                    if (!_key.StartsWith(_foundKey))
                    {
                        _foundKey = string.Empty;
                    }
                }
            }
        }

        private byte[] GetKeyWithoutEnd(List<byte> part)
        {
            var n = part.Count;
            var result = part;

            if (n >= 2 && part[n - 2] == 0 && part[n - 1] == 1)
            {
                result = part.Take(n - 2).ToList();
            }

            return RemoveZeros(result).ToArray();
        }

        private IEnumerable<byte> RemoveZeros(List<byte> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i < source.Count - 1 && source[i] == 0 && source[i + 1] == 0) i++;

                yield return source[i];
            }
        }

        private IEnumerable<byte[]> Split(byte[] buff)
        {
            var prev = 0;

            for (var i = 0; i < buff.Length - 1;)
            {
                if (buff[i] == 0 && buff[i + 1] == 0)
                {
                    i += 2;
                    continue;
                }

                if (buff[i] == 0 && buff[i + 1] == 1)
                {
                    yield return buff.Skip(prev).Take(i + 2 - prev).ToArray();
                    prev = i + 2;
                    i++;
                }

                i++;
            }

            yield return buff.Skip(prev).Take(buff.Length - prev).ToArray();
        }

        public override void Flush()
        {
            // nothing to do
        }
    }
}