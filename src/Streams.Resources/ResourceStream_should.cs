using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Streams.Resources
{
    [TestFixture]
    public class ResourceStream_should
    {
        private readonly Dictionary<string, string> data = new Dictionary<string, string>
        {
            ["fileList.txt"] = "Value1",
            ["EmptyImage.png"] = "",
            ["mapObjectsList.txt"] = new string('a', 10000),
            ["ogreMagiAnimation.gif"] = new string('a', 50000) + "bcd" + new string('e', 50000),
            ["ogreMagi.png"] = "SomeValueWith \0 byte",
            ["white\0Wolf.png"] = "Zero value in the key",
            ["wolf.png"] = GetStringByBytes(68, 69, 0, 1, 70, 71)
        };

        private IEnumerable<string> GetKeysAndValues() => data.SelectMany(z => new[] {z.Key, z.Value});

        private TestStream testStream;

        [SetUp]
        public void SetUp()
        {
            testStream = new TestStream(GetKeysAndValues());
        }

        [Test]
        public void ReadSimpleKey()
        {
            AssertReadKeyEqualsData("fileList.txt", testStream);
        }

        [Test]
        public void ReadEmptyValue()
        {
            AssertReadKeyEqualsData("EmptyImage.png", testStream);
        }

        [Test]
        public void ReadLongValue()
        {
            AssertReadKeyEqualsData("mapObjectsList.txt", testStream);
        }

        [Test]
        public void ReadVeryLongValue()
        {
            AssertReadKeyEqualsData("ogreMagiAnimation.gif", testStream);
        }

        [Test]
        public void ReadsCorrectly_WhenZeroValueInTheValue()
        {
            AssertReadKeyEqualsData("ogreMagi.png", testStream);
        }

        [Test]
        public void ReadsCorrectly_WhenZeroValueInTheKey()
        {
            AssertReadKeyEqualsData("white\0Wolf.png", testStream);
        }

        [Test]
        public void ReadsCorrectly_WhenSeparatorInTheKey()
        {
            var key = GetStringByBytes(68, 69, 0, 1, 70, 71);
            var value = "Separator value in the key";
            testStream = new TestStream(new[] {key, value});
            AssertReadKeyEqualsData(key, testStream, value);
        }

        [Test]
        public void ReadsCorrectly_WhenSeparatorInTheValue()
        {
            AssertReadKeyEqualsData("wolf.png", testStream);
        }

        [Test]
        public void ReadsCorrectly_WhenSeparatorOnTheBorder()
        {
            var keyLength = Constants.BufferSize / 2;
            var key = new string('k', keyLength);
            var value = new string('v', Constants.BufferSize - keyLength - 1);
            testStream = new TestStream(new[] {key, value});
            AssertReadKeyEqualsData(key, testStream, value);
        }

        [Test]
        public void ReadsCorrectly_WhenZeroOnTheBorder()
        {
            var keyLength = Constants.BufferSize / 2;
            var key = new string('u', keyLength);
            var value = new string('c', Constants.BufferSize - keyLength - 1) + "\0HELLO";
            testStream = new TestStream(new[] {key, value});
            AssertReadKeyEqualsData(key, testStream, value);
        }

        [Test]
        public void ReadsCorrectly_LongKey()
        {
            var key = new string('k', Constants.BufferSize + 1);
            var value = new string('v', Constants.BufferSize / 2);
            testStream = new TestStream(new[] {key, value});
            AssertReadKeyEqualsData(key, testStream, value);
        }

        [Test]
        public void ThrowsException_WhenReadUnknownKey()
        {
            AssertThrowsExceptionWhenReadKey("unknown", testStream);
        }

        [TestCase(new byte[] {0, 1}, TestName = "No value after key")]
        [TestCase(new byte[] {0}, TestName = "Not finished separator")]
        [TestCase(new byte[0], TestName = "No separator after key")]
        public void ThrowsException_WhenUnexpectedEndOfStream_InKey(byte[] keyEnding)
        {
            var key = "mainHero.png";
            testStream = new TestStream(new[] {key}, keyEnding);
            AssertThrowsExceptionWhenReadKey(key, testStream);
        }

        [TestCase(new byte[] {0}, TestName = "Not finished separator")]
        [TestCase(new byte[0], TestName = "No separator after value")]
        public void ThrowsException_WhenUnexpectedEndOfStream_InValue(byte[] valueEnding)
        {
            var key = "butcher.png";
            var keyBytes = Encoding.ASCII.GetBytes(key);
            var streamBytes = keyBytes
                .Concat(new byte[] {0, 1})
                .Concat(new byte[] {1, 2, 3, 4, 5})
                .Concat(valueEnding)
                .ToArray();
            testStream = new TestStream(streamBytes);
            AssertThrowsExceptionWhenReadKey(key, testStream);
        }

        [Test]
        public void WorksCorrectly_WithInfiniteStream()
        {
            testStream = new TestStream(GetKeysAndValues(), infinityMode: true);
            AssertReadKeyEqualsData("mapObjectsList.txt", testStream);
        }

        [Test]
        public void ThrowsNotSupportedException_OnWriteAndSeekOperations()
        {
            var reader = new ResourceReaderStream(testStream, data.Keys.FirstOrDefault());
            Assert.Throws<NotSupportedException>(() => reader.Seek(0, SeekOrigin.Current));
            Assert.Throws<NotSupportedException>(() => reader.SetLength(0));
            Assert.Throws<NotSupportedException>(() => reader.Write(new byte[10], 0, 10));
            Assert.Throws<NotSupportedException>(() =>
            {
                var _ = reader.Length;
            });
            Assert.Throws<NotSupportedException>(() => reader.Position++);
            Assert.AreEqual(false, reader.CanSeek);
            Assert.AreEqual(false, reader.CanWrite);
            Assert.AreEqual(true, reader.CanRead);
        }

        private void AssertReadKeyEqualsData(string key, TestStream stream)
        {
            AssertReadKeyEqualsData(key, stream, data[key]);
        }

        private void AssertReadKeyEqualsData(string key, TestStream stream, string expectedValue)
        {
            var result = Read(key, stream);
            var str = Encoding.ASCII.GetString(result.ToArray());
            Assert.AreEqual(expectedValue, str);
        }

        private void AssertThrowsExceptionWhenReadKey(string key, TestStream stream)
        {
            Assert.Throws<EndOfStreamException>(() => Read(key, stream));
        }

        private List<byte> Read(string key, TestStream stream)
        {
            var reader = new ResourceReaderStream(stream, key);
            var buffer = new byte[100];
            var result = new List<byte>();
            while (true)
            {
                var count = reader.Read(buffer, 0, 100);
                if (count == 0) break;
                result.AddRange(buffer.Take(count));
            }

            return result;
        }

        private static string GetStringByBytes(params byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}