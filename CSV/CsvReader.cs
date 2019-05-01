using MatthiWare.Csv.Abstractions;
using MatthiWare.Csv.Core;
using MatthiWare.Csv.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MatthiWare.Csv
{
    public class CsvReader : ICsvReader
    {
        protected readonly CsvDeserializer m_serializer;

        public bool HasData => !m_serializer.EndReached;

        public CsvConfig Config { get; }

        public IReadOnlyCollection<string> Headers => m_serializer.Headers;

        public CsvReader(string filePath, CsvConfig config = null)
            : this(File.OpenRead(Guard.CheckNotNull(filePath, nameof(filePath))), config)
        {

        }

        public CsvReader(Stream input, CsvConfig config = null)
        {
            Config = config ?? new CsvConfig();

            m_serializer = new CsvDeserializer(
                Config.IsStreamOwner ? input : new NonClosableStream(input),
                Config);
        }

        public IEnumerable<ICsvDataRow> ReadRows()
        {
            while (!m_serializer.EndReached)
                yield return m_serializer.ReadRow();
        }

        public IEnumerable<Task<ICsvDataRow>> ReadRowsAsync()
        {
            while (!m_serializer.EndReached)
                yield return m_serializer.ReadRowAsync();
        }

        public ICsvDataRow ReadRow() => m_serializer.ReadRow();

        public Task<ICsvDataRow> ReadRowAsync() => m_serializer.ReadRowAsync();


        public IEnumerable<T> ReadRows<T>() where T : class, new()
        {
            while (!m_serializer.EndReached)
                yield return m_serializer.ReadRow<T>();
        }

        public IEnumerable<Task<T>> ReadRowsAsync<T>() where T : class, new()
        {
            while (!m_serializer.EndReached)
                yield return m_serializer.ReadRowAsync<T>();
        }

        public T ReadRow<T>() where T : class, new()
            => m_serializer.ReadRow<T>();

        public Task<T> ReadRowAsync<T>() where T : class, new()
            => m_serializer.ReadRowAsync<T>();

        public void Dispose()
        {
            m_serializer.Dispose();
        }
    }
}
