using MatthiWare.Csv;
using SampleCsvReaderApp.Models;
using System;

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
                var x = reader.Headers;
                Console.WriteLine(string.Join(" ", x));

                foreach (var record in reader.ReadRows<SampleModel>())
                {
                    Console.WriteLine($"ID: {record.ID}; County: {record.County}; Limit: {record.Limit}; Granularity: {record.Granularity}");
                }
            }
        }
    }
}
