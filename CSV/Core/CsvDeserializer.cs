using MatthiWare.Csv.Core.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MatthiWare.Csv.Core
{
    public class CsvDeserializer : IDisposable
    {
        private readonly CsvConfig m_config;
        private readonly StreamReader m_reader;
        private readonly List<string> m_headers = new List<string>();
        private readonly Mapper m_mapper;

        private bool m_firstLine = true;

        public bool EndReached => Guard.CheckEndOfStream(m_reader);

        public CsvDeserializer(Stream reader, CsvConfig config)
        {
            m_reader = new StreamReader(reader);
            m_config = config;
            m_mapper = new Mapper();

            CheckHeader();
        }

        public void Reset()
        {
            m_headers.Clear();
            m_firstLine = true;
            m_reader.BaseStream.Seek(0, SeekOrigin.Begin);
            m_reader.DiscardBufferedData();

            CheckHeader();
        }

        #region Headers

        private void CheckHeader()
        {
            if (!m_firstLine)
                return;

            var tokens = GetNextTokens();

            if (!m_config.FirstLineIsHeader && m_config.GenerateDefaultHeadersIfNotFound)
                GetDefaultHeaders(tokens.Length);
            else if (m_config.FirstLineIsHeader)
                GetHeaders(tokens);
        }

        private void GetHeaders(string[] tokens)
        {
            foreach (var header in tokens)
                m_headers.Add(header);

            m_firstLine = false;
        }

        public IReadOnlyCollection<string> Headers => m_headers;

        private void GetDefaultHeaders(int length)
        {
            for (int i = 0; i < length; i++)
                m_headers.Add($"column {i + 1}");

            m_reader.BaseStream.Seek(0, SeekOrigin.Begin);
            m_reader.DiscardBufferedData();
        }

        #endregion

        public ICsvDataRow ReadRow() => CreateDataRow(GetNextTokens());

        public async Task<ICsvDataRow> ReadRowAsync() => CreateDataRow(await GetNextTokensAsync());

        private ICsvDataRow CreateDataRow(string[] tokens)
        {
            var items = new List<CsvDataItem>();

            for (int i = 0; i < tokens.Length; i++)
            {
                items.Add(new CsvDataItem(m_headers[i], tokens[i]));
            }

            return new CsvDataRow(items);
        }

        public T ReadRow<T>()
            where T : class, new()
        {
            var model = CreateModel<T>();
            var tokens = GetNextTokens();

            m_mapper.Map(model, m_headers, tokens);

            return model;
        }

        public async Task<T> ReadRowAsync<T>()
            where T : class, new()
        {
            var tokens = GetNextTokensAsync();
            var model = CreateModel<T>();

            m_mapper.Map(model, m_headers, await tokens);

            return model;
        }

        private T CreateModel<T>()
            where T : class, new()
        {
            if (typeof(T) is object)
                return CreateDynamicModel();

            return (T)Activator.CreateInstance(typeof(T));
        }

        private dynamic CreateDynamicModel()
        {
            var instance = new DynamicModel();

            foreach (var key in m_headers)
                instance.AddProperty(key, null);

            return instance;
        }

        private async Task<string[]> GetNextTokensAsync()
        {
            var tokens = await m_reader.ReadLineAsync();

            return tokens.Split(m_config.ValueSeperator);
        }

        private string[] GetNextTokens()
            => m_reader.ReadLine().Split(m_config.ValueSeperator);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_reader?.Dispose();

                    m_headers?.Clear();

                    if (m_config.ReleaseCacheOnDispose)
                        m_mapper?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
