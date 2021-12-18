using MatthiWare.Csv;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV.Tests
{
    [TestFixture]
    public class CsvReaderTests
    {
        [Test]
        public async Task ReadAsyncRows()
        {
            var array = Enumerable.Empty<string[]>()
                .Append(new[] { "1", "2", "3" })
                .Append(new[] { "4", "5", "6" })
                .Append(new[] { "7", "8", "9" });

            var file = GenerateCustomInMemoryCsvFile(null, array);

            using (var reader = new CsvReader(file, new CsvConfig { FirstLineIsHeader = false }))
            {
                var count = 0;
                var arr = array.ToArray();
                foreach (var record in reader.ReadRowsAsync())
                {
                    Assert.AreEqual(arr[count++], (await record).Values.ToArray());
                }

                Assert.False(reader.HasData);
            }
        }

        [Test]
        public void ReadsCorrectHeaders()
        {
            var originalHeaders = new[] { "header1", "header2" };
            var file = GenerateCustomInMemoryCsvFile(originalHeaders, Enumerable.Repeat(new[] { "1", "2", "3" }, 3));

            using (var reader = new CsvReader(file))
            {
                var headers = reader.Headers;

                Assert.AreEqual(originalHeaders, headers);
            }
        }

        [Test]
        public void ReadsCorrectGeneratedHeaders()
        {
            var originalHeaders = new[] { "column 1", "column 2", "column 3" };
            var file = GenerateCustomInMemoryCsvFile(null, Enumerable.Repeat(new[] { "1", "2", "3" }, 3));

            var config = new CsvConfig { FirstLineIsHeader = false };

            using (var reader = new CsvReader(file, config))
            {
                var headers = reader.Headers;

                Assert.AreEqual(originalHeaders, headers);
            }
        }

        [Test]
        public void CheckCsvReaderContentIsCorrect()
        {
            var origContent = Enumerable.Empty<string[]>()
                .Append(new[] { "1", "2", "3" })
                .Append(new[] { "4", "5", "6" })
                .Append(new[] { "7", "8", "9" });

            var origContentArr = origContent.ToArray();
            var file = GenerateCustomInMemoryCsvFile(null, origContent);

            var config = new CsvConfig { FirstLineIsHeader = false };

            using (var reader = new CsvReader(file, config))
            {

                var actualContent = reader.ReadRows().Select(row => row.Values).ToArray();
                var content = actualContent.ToArray();

                var same = (origContent.Count() == actualContent.Count());
                var same2 = (!origContent.Except(actualContent).Any() || !actualContent.Except(origContent).Any());


                Assert.IsTrue(same);
            }
        }

        [Theory]
        [TestCase(true)]
        [TestCase(false)]
        public void CheckCsvReaderContentIsCorrect(bool async)
        {
            var headers = new[] { "header_1", "header_2", "header_3" };
            var row1 = new[] { "1", "2", "3" };
            var row2 = new[] { "4", "5", "6" };
            var row3 = new[] { "7", "8", "9" };

            var origContent = Enumerable.Empty<string[]>()
                .Append(headers)
                .Append(row1)
                .Append(row2)
                .Append(row3);

            var origContentArr = origContent.ToArray();
            var file = GenerateCustomInMemoryCsvFile(null, origContent);

            using (var reader = new CsvReader(file))
            {
                Assert.AreEqual(headers, reader.Headers);

                if (async)
                {
                    Assert.AreEqual(row1, reader.ReadRowAsync().Result.Values.ToArray());
                    Assert.AreEqual(row2, reader.ReadRowAsync().Result.Values.ToArray());
                    Assert.AreEqual(row3, reader.ReadRowAsync().Result.Values.ToArray());
                }
                else
                {
                    Assert.AreEqual(row1, reader.ReadRow().Values.ToArray());
                    Assert.AreEqual(row2, reader.ReadRow().Values.ToArray());
                    Assert.AreEqual(row3, reader.ReadRow().Values.ToArray());
                }

                Assert.False(reader.HasData);
            }
        }



        private MemoryStream GenerateCustomInMemoryCsvFile(string[] headers, IEnumerable<string[]> fields, string split = ",")
        {
            var memory = new MemoryStream();
            var sw = new StreamWriter(memory);
            var sb = new StringBuilder();
            var hasHeaders = headers != null;

            if (hasHeaders)
            {
                for (int i = 0; i < headers.Length; i++)
                    sb.Append(headers[i]).Append(i == headers.Length - 1 ? string.Empty : split);

                var header = sb.ToString();
                sw.WriteLine(header);

                sb.Clear();
            }

            foreach (var row in fields)
            {
                for (int i = 0; i < row.Length; i++)
                    sb.Append(row[i]).Append(i == row.Length - 1 ? string.Empty : split);

                sw.WriteLine(sb.ToString());

                sb.Clear();
            }

            sw.Flush();

            memory.Position = 0;

            return memory;
        }
    }
}
