using System;

using MatthiWare.Csv;

namespace SampleCsvReaderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Read("./sample.csv");

            Console.ReadKey();
        }

        static void Read(string csv)
        {
            using (var reader = new CsvReader(csv))
            {
                var x = reader.GetHeaders();
                Console.WriteLine(string.Join(" ", x));

                foreach (var record in reader.ReadRecords())
                {
                    Console.WriteLine($"PolicyID: {record["policyID"]}");
                    Console.WriteLine($"Dynamic Line: {record.DynamicContent.line} Construction: {record.DynamicContent.construction}");
                }
            }
        }
    }
}
