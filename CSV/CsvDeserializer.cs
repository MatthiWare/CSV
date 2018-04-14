using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MatthiWare.Csv
{
    internal class CsvDeserializer<T> : IDisposable where T : new()
    {
        private readonly CsvConfig m_config;
        private readonly StreamReader m_reader;
        private readonly Dictionary<string, int> m_headers = new Dictionary<string, int>();
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
            int count = 0;

            foreach (var header in tokens)
                m_headers.Add(header, count++);

            m_firstLine = false;
        }

        public string[] GetHeaders() => m_headers.Keys.ToArray();

        private void GetDefaultHeaders(int length)
        {
            for (int i = 0; i < length; i++)
                m_headers.Add($"column {i + 1}", i);

            m_reader.BaseStream.Seek(0, SeekOrigin.Begin);
            m_reader.DiscardBufferedData();
        }

        #endregion

        public T DeserializeRow()
        {
            var model = CreateModel();
            var tokens = GetNextTokens();

            m_mapper.Map(model, m_headers, tokens);

            return model;
        }

#if DOT_NET_STD
        public async Task<T> DeserializeRowAsync()
        {
            var tokens = GetNextTokensAsync();
            var model = CreateModel();

            m_mapper.Map(model, m_headers, await tokens);

            return model;
        }
#endif

        private T CreateModel() => (T)Activator.CreateInstance(typeof(T));

#if DOT_NET_STD
        private async Task<string[]> GetNextTokensAsync()
        {
            var tokens = await m_reader.ReadLineAsync();

            return tokens.Split(m_config.ValueSeperator);
        }
#endif

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
                        m_mapper?.ReleaseCache();
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
