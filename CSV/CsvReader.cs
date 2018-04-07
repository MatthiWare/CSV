using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if DOT_NET_STD
using System.Threading.Tasks;
#endif

namespace MatthiWare.Csv
{
    public class CsvReader<T> : IEnumerable<T>, IDisposable where T : new()
    {
        public CsvConfig Config { get; private set; }

        private readonly CsvDeserializer<T> m_serializer;

        public bool EndReached => m_serializer?.EndReached ?? true;

        public CsvReader(string filePath, CsvConfig config = null)
            : this(File.OpenRead(Guard.CheckNotNull(filePath, nameof(filePath))), config)
        {

        }

        public CsvReader(Stream input, CsvConfig config = null)
        {
            Config = config ?? new CsvConfig();

            m_serializer = new CsvDeserializer<T>(
                Config.IsStreamOwner ?
                input :
                new NonClosableStream(input),
                Config);
        }

        #region Public API

        public string[] GetHeaders() => m_serializer.GetHeaders();

        public IEnumerable<T> ReadRecords()
        {
            foreach (var record in this)
                yield return record;
        }

#if DOT_NET_STD
        public async Task<T> ReadNextRowAsync() => await m_serializer.DeserializeRowAsync();
#endif

        public T ReadNextRow() => m_serializer.DeserializeRow();

        public void Reset() => m_serializer.Reset();

        #endregion

        #region IEnumerable Support

        public IEnumerator<T> GetEnumerator() => new CsvReaderEnumerator<T>(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Config = null;

                    m_serializer?.Dispose();
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

        //private class CsvDataRow : ICsvDataRow
        //{
        //    private readonly string[] m_values;

        //    private Lazy<dynamic> m_lazyDynamicContent;

        //    private readonly CsvReader<T> m_parent;

        //    public CsvDataRow(string[] values, CsvReader<T> reader)
        //    {
        //        m_values = values;

        //        m_parent = reader;

        //        m_lazyDynamicContent = new Lazy<dynamic>(CreateDynamicContent);
        //    }

        //    public string this[string name]
        //    {
        //        get
        //        {
        //            if (m_parent.m_headers.Count == 0)
        //                throw new InvalidOperationException($"No headers provided in the csv file and the {nameof(CsvConfig.GenerateDefaultHeadersIfNotFound)} in the {nameof(CsvConfig)} has been turned off.");

        //            if (!m_parent.m_headers.TryGetValue(name, out int index))
        //                throw new ArgumentException($"Header {name} does not exist.");

        //            return Values[index];
        //        }
        //    }

        //    public string this[int index] => Values[index];

        //    public string[] Values => m_values;

        //    public dynamic DynamicContent => m_lazyDynamicContent.Value;

        //    private dynamic CreateDynamicContent()
        //    {
        //        var expando = new ExpandoObject();
        //        var dict = (IDictionary<string, object>)expando;

        //        int count = 0;

        //        foreach (var header in m_parent.m_headers.Keys)
        //            dict.Add(header, m_values[count++]);

        //        return expando;
        //    }
        //}

        private class CsvReaderEnumerator<TModel> : IEnumerator<TModel> where TModel : new()
        {
            private TModel m_current;

            public TModel Current => m_current;

            object IEnumerator.Current => m_current;

            private readonly CsvReader<TModel> m_reader;

            public CsvReaderEnumerator(CsvReader<TModel> reader)
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

    public class CsvReader : CsvReader<dynamic>
    {
        public CsvReader(string filePath, CsvConfig config = null)
            : base(filePath, config)
        { }

        public CsvReader(Stream input, CsvConfig config = null)
            : base(input, config)
        { }
    }
}
