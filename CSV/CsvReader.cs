using MatthiWare.Csv.Abstractions;
using MatthiWare.Csv.Core;
using MatthiWare.Csv.Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MatthiWare.Csv
{
    public class CsvReader : ICsvReader
    {
        private readonly CsvDeserializer reader;

        public bool HasData => !reader.EndReached;

        public CsvConfig Config { get; }

        public IReadOnlyCollection<string> Headers => reader.GetHeaders();

        public CsvReader(string filePath, CsvConfig config = null)
            : this(File.OpenRead(Guard.CheckNotNull(filePath, nameof(filePath))), config)
        {

        }

        public CsvReader(Stream input, CsvConfig config = null)
        {
            Config = config ?? new CsvConfig();

            reader = new CsvDeserializer(input, Config);
        }

        public IEnumerable<ICsvDataRow> ReadRows()
        {
            reader.ReadHeaders();

            while (!reader.EndReached)
            {
                yield return reader.ReadRow();
            }
        }

        public IEnumerable<Task<ICsvDataRow>> ReadRowsAsync()
        {
            reader.ReadHeaders();

            while (!reader.EndReached)
            {
                yield return reader.ReadRowAsync();
            }
        }

        public ICsvDataRow ReadRow()
        {
            reader.ReadHeaders();
            return reader.ReadRow();
        }

        public Task<ICsvDataRow> ReadRowAsync()
        {
            reader.ReadHeaders();
            return reader.ReadRowAsync();
        }

        public IEnumerable<T> ReadRows<T>() where T : class, new()
        {
            reader.ReadHeaders();

            while (!reader.EndReached)
            {
                yield return reader.ReadRow<T>();
            }
        }

        public IEnumerable<Task<T>> ReadRowsAsync<T>() where T : class, new()
        {
            reader.ReadHeaders();

            while (!reader.EndReached)
            {
                yield return reader.ReadRowAsync<T>();
            }
        }

        public T ReadRow<T>() where T : class, new()
        {
            reader.ReadHeaders();
            return reader.ReadRow<T>();
        }

        public Task<T> ReadRowAsync<T>() where T : class, new()
        {
            reader.ReadHeaders();
            return reader.ReadRowAsync<T>();
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
