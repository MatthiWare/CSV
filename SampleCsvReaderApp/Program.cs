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
                foreach (var record in reader.ReadRecords())
                {
                    Console.WriteLine(record["policyID"]);
                }
            }
        }
    }
}
