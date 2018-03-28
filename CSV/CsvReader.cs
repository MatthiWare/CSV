using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace MatthiWare.Csv
{
    public class CsvReader : IEnumerable<ICsvDataRow>, IDisposable
    {

        private StreamReader m_streamReader;
        private Lazy<IEnumerator<ICsvDataRow>> m_lazyEnum;

        private CsvConfig m_config;

        private CsvReader(CsvConfig config)
        {
            m_config = config ?? new CsvConfig();
        }

        public CsvReader(string filePath, CsvConfig config = null)
            : this(config)
        {

            var stream = File.OpenRead(Guard.CheckNotNull(filePath, nameof(filePath)));

            m_streamReader = new StreamReader(stream);

            Init();
        }

        public CsvReader(StreamReader reader, CsvConfig config = null)
            : this(config)
        {
            m_streamReader = Guard.CheckNotNull(reader, nameof(reader));

            Init();
        }

        public CsvReader(Stream inStream, CsvConfig config = null)
            : this(config)
        {
            m_streamReader = new StreamReader(Guard.CheckNotNull(inStream, nameof(inStream)));

            Init();
        }

        private void Init()
        {
            m_lazyEnum = new Lazy<IEnumerator<ICsvDataRow>>(() => new CsvReaderEnumerator(m_streamReader, m_config));
        }

        public string[] GetHeaders()
        {
            if (!m_config.FirstLineIsHeader && !m_config.GenerateDefaultHeadersIfNotFound)
                throw new InvalidOperationException($"No headers provided in the csv file and the {nameof(CsvConfig.GenerateDefaultHeadersIfNotFound)} in the {nameof(CsvConfig)} has been turned off.");

            var enumerator = (CsvReaderEnumerator)m_lazyEnum.Value;

            return enumerator.GetHeaders();
        }

        public IEnumerable<ICsvDataRow> ReadRecords()
        {
            while (m_lazyEnum.Value.MoveNext())
                yield return m_lazyEnum.Value.Current;
        }

        public IEnumerator<ICsvDataRow> GetEnumerator() => m_lazyEnum.Value;

        IEnumerator IEnumerable.GetEnumerator() => m_lazyEnum.Value;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (m_lazyEnum.IsValueCreated)
                        m_lazyEnum.Value.Dispose();

                    m_config = null;
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
            private readonly Dictionary<string, int> m_headers;

            private Lazy<dynamic> m_lazyDynamicContent;

            public CsvDataRow(string[] values, Dictionary<string, int> headers)
            {
                m_values = values;
                m_headers = headers;

                m_lazyDynamicContent = new Lazy<dynamic>(CreateDynamicContent);
            }

            public string this[string name]
            {
                get
                {
                    if (m_headers.Count == 0)
                        throw new InvalidOperationException($"No headers provided in the csv file and the {nameof(CsvConfig.GenerateDefaultHeadersIfNotFound)} in the {nameof(CsvConfig)} has been turned off.");

                    if (!m_headers.TryGetValue(name, out int index))
                        throw new ArgumentException($"Header {name} does not exist.");

                    return Values[index];
                }
            }

            public string this[int index] => Values[index];

            public string[] Values => m_values;

            public dynamic DynamicContent => m_lazyDynamicContent.Value;

            public string[] Headers
            {
                get
                {
                    if (m_headers.Count == 0)
                        throw new InvalidOperationException($"No headers provided in the csv file and the {nameof(CsvConfig.GenerateDefaultHeadersIfNotFound)} in the {nameof(CsvConfig)} has been turned off.");

                    return m_headers.Keys.ToArray();
                }
            }

            private dynamic CreateDynamicContent()
            {
                var expando = new ExpandoObject();
                var dict = (IDictionary<string, object>)expando;

                int count = 0;

                foreach (var header in m_headers.Keys)
                    dict.Add(header, m_values[count++]);

                return expando;
            }
        }

        private class CsvReaderEnumerator : IEnumerator<ICsvDataRow>
        {
            private ICsvDataRow m_current;

            public ICsvDataRow Current => m_current;

            object IEnumerator.Current => m_current;

            private readonly CsvConfig m_config;
            private readonly StreamReader m_reader;
            private Dictionary<string, int> m_headers;

            private bool m_firstLine = true;

            public CsvReaderEnumerator(StreamReader reader, CsvConfig config)
            {
                m_reader = reader;
                m_config = config;
                m_headers = new Dictionary<string, int>();
            }

            public string[] GetHeaders()
            {
                if (m_headers.Count == 0)
                    CheckHeader();

                return m_headers.Keys.ToArray();
            }

            public void Dispose()
            {
                if (m_config.IsStreamOwner)
                    m_reader.Dispose();
            }

            public bool MoveNext()
            {
                if (Guard.CheckEndOfStream(m_reader))
                    return false;

                CheckHeader();

                m_current = ReadRow(GetNextTokens());

                return true;
            }

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

            private string[] GetNextTokens()
                => m_reader.ReadLine().Split(m_config.ValueSeperator);

            private ICsvDataRow ReadRow(string[] tokens)
                => new CsvDataRow(tokens, m_headers);

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
                    m_headers.Add($"column {i}", i);

                m_reader.BaseStream.Position = 0;
            }

            public void Reset()
            {
                m_reader.BaseStream.Position = 0;
                m_current = null;
                m_firstLine = true;
                m_headers.Clear();
            }
        }
    }
}
