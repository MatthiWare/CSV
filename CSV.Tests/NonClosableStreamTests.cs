using System;
using System.IO;

using MatthiWare.Csv;

using NUnit.Framework;

namespace CSV.Tests
{
    [TestFixture]
    public class NonClosableStreamTests
    {


        [Test]
        public void DoesNotDisposeUnderlyingStream()
        {
            var original = new MemoryStream();

            var swOriginal = new StreamWriter(original);

            swOriginal.WriteLine("Test");
            swOriginal.Flush();
            original.Position = 0;

            using (var nonClosable = new NonClosableStream(original))
            {

            }

            var srAfter = new StreamReader(original);

            var output = srAfter.ReadLine();

            Assert.AreEqual("Test", output);
        }

        [Test]
        public void ClosingMakesTheStreamWrapperUnusable()
        {
            var stream = new NonClosableStream(new MemoryStream());

            using (stream) { }

            Assert.Throws<InvalidOperationException>(() => stream.WriteByte(1));
        }
    }
}
