using System;

using MatthiWare.Csv;

using SampleCsvReaderApp.Models;

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
            using (var reader = new CsvReader<SampleModel>(csv))
            {
                var x = reader.GetHeaders();
                Console.WriteLine(string.Join(" ", x));

                foreach (var record in reader.ReadRecords())
                {
                    Console.WriteLine($"ID: {record.ID}; County: {record.County}; Limit: {record.Limit}; Granularity: {record.Granularity}");
                }
            }
        }
    }
}
