using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MatthiWare.Csv
{
    public class CsvReader : IEnumerable<ICsvDataRow>, IDisposable
    {
        private StreamReader m_reader;

        public CsvConfig Config { get; private set; }

        private Dictionary<string, int> m_headers;

        private bool m_firstLine = true;

        public bool EndReached => Guard.CheckEndOfStream(m_reader);

        private CsvReader(CsvConfig config)
        {
            Config = config ?? new CsvConfig();
            m_headers = new Dictionary<string, int>();
        }

        public CsvReader(string filePath, CsvConfig config = null)
            : this(config)
        {
            var stream = File.OpenRead(Guard.CheckNotNull(filePath, nameof(filePath)));

            m_reader = new StreamReader(stream);

            CheckHeader();
        }

        public CsvReader(Stream inStream, CsvConfig config = null)
            : this(config)
        {
            m_reader = new StreamReader(Guard.CheckNotNull(inStream, nameof(inStream)));

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

            if (!Config.FirstLineIsHeader && Config.GenerateDefaultHeadersIfNotFound)
                GetDefaultHeaders(tokens.Length);
            else if (Config.FirstLineIsHeader)
                GetHeaders(tokens);
        }

        private void GetHeaders(string[] tokens)
        {
            int count = 0;

            foreach (var header in tokens)
                m_headers.Add(header, count++);

            m_firstLine = false;
        }

        private void GetDefaultHeaders(int length)
        {
            for (int i = 0; i < length; i++)
                m_headers.Add($"column {i + 1}", i);

            m_reader.BaseStream.Seek(0, SeekOrigin.Begin);
            m_reader.DiscardBufferedData();
        }

        #endregion

#if ASYNC_FEATURE
        private async Task<string[]> GetNextTokensAsync()
        {
            var tokens = await m_reader.ReadLineAsync();

            return tokens.Split(Config.ValueSeperator);
        }
#endif

        private string[] GetNextTokens()
            => m_reader.ReadLine().Split(Config.ValueSeperator);

        #region Public API

        public string[] GetHeaders()
        {
            if (!Config.FirstLineIsHeader && !Config.GenerateDefaultHeadersIfNotFound)
                throw new InvalidOperationException($"No headers provided in the csv file and the {nameof(CsvConfig.GenerateDefaultHeadersIfNotFound)} in the {nameof(CsvConfig)} has been turned off.");

            return m_headers.Keys.ToArray();
        }

        public ICsvDataRow ReadNextRow() => DeserializeRow(GetNextTokens());

#if ASYNC_FEATURE
        public async Task<ICsvDataRow> ReadNextRowAsync() => DeserializeRow(await GetNextTokensAsync());
#endif

        public IEnumerable<ICsvDataRow> ReadRecords()
        {
            foreach (var record in this)
                yield return record;
        }

        #endregion

        private ICsvDataRow DeserializeRow(string[] tokens) => new CsvDataRow(tokens, this);

        public IEnumerator<ICsvDataRow> GetEnumerator() => new CsvReaderEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Config = null;

                    if (m_reader != null)
                    {
                        m_reader.Dispose();
                        m_reader = null;
                    }
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

        private class CsvDataRow : ICsvDataRow
        {
            private readonly string[] m_values;

            private Lazy<dynamic> m_lazyDynamicContent;

            private readonly CsvReader m_parent;

            public CsvDataRow(string[] values, CsvReader reader)
            {
                m_values = values;

                m_parent = reader;

                m_lazyDynamicContent = new Lazy<dynamic>(CreateDynamicContent);
            }

            public string this[string name]
            {
                get
                {
                    if (m_parent.m_headers.Count == 0)
                        throw new InvalidOperationException($"No headers provided in the csv file and the {nameof(CsvConfig.GenerateDefaultHeadersIfNotFound)} in the {nameof(CsvConfig)} has been turned off.");

                    if (!m_parent.m_headers.TryGetValue(name, out int index))
                        throw new ArgumentException($"Header {name} does not exist.");

                    return Values[index];
                }
            }

            public string this[int index] => Values[index];

            public string[] Values => m_values;

            public dynamic DynamicContent => m_lazyDynamicContent.Value;

            private dynamic CreateDynamicContent()
            {
                var expando = new ExpandoObject();
                var dict = (IDictionary<string, object>)expando;

                int count = 0;

                foreach (var header in m_parent.m_headers.Keys)
                    dict.Add(header, m_values[count++]);

                return expando;
            }
        }

        private class CsvReaderEnumerator : IEnumerator<ICsvDataRow>
        {
            private ICsvDataRow m_current;

            public ICsvDataRow Current => m_current;

            object IEnumerator.Current => m_current;

            private readonly CsvReader m_reader;

            public CsvReaderEnumerator(CsvReader reader)
            {
                m_reader = reader;

                m_reader.Reset();
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                if (m_reader.EndReached)
                    return false;

                m_current = m_reader.ReadNextRow();

                return true;
            }

            public void Reset()
            {

            }
        }
    }
}
