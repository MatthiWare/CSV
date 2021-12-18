using MatthiWare.Csv.Abstractions;
using MatthiWare.Csv.Core.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MatthiWare.Csv.Core
{
    public class CsvDeserializer : IDisposable
    {
        private readonly CsvConfig config;
        private readonly StreamReader reader;
        private readonly List<string> headers = new();
        private readonly Mapper mapper;

        private bool headersRead = false;

        public bool EndReached => Guard.CheckEndOfStream(reader);

        public CsvDeserializer(Stream reader, CsvConfig config)
        {
            this.reader = new StreamReader(reader);
            this.config = config;
            mapper = new Mapper();
        }

        public void Reset()
        {
            headers.Clear();
            headersRead = false;
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();
        }

        #region Headers

        public void ReadHeaders()
        {
            if (headersRead)
            {
                return;
            }

            var tokens = GetNextTokens();

            if (!config.FirstLineIsHeader)
            {
                GetDefaultHeaders(tokens.Length);
            }
            else
            {
                GetHeaders(tokens);
            }
        }

        private void GetHeaders(string[] tokens)
        {
            foreach (var header in tokens)
            {
                headers.Add(header);
            }

            headersRead = true;
        }

        public IReadOnlyCollection<string> GetHeaders()
        {
            ReadHeaders();

            return headers;
        }

        private void GetDefaultHeaders(int length)
        {
            for (int i = 0; i < length; i++)
            {
                headers.Add($"header {i + 1}");
            }

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();

            headersRead = true;
        }

        #endregion

        public ICsvDataRow ReadRow() => CreateDataRow(GetNextTokens());

        public async Task<ICsvDataRow> ReadRowAsync() => CreateDataRow(await GetNextTokensAsync());

        private ICsvDataRow CreateDataRow(string[] tokens)
        {
            var items = new List<CsvDataItem>();

            for (int i = 0; i < tokens.Length; i++)
            {
                items.Add(new CsvDataItem(headers[i], tokens[i]));
            }

            return new CsvDataRow(items);
        }

        public T ReadRow<T>()
            where T : class, new()
        {
            var tokens = GetNextTokens();
            var model = CreateModel<T>();

            mapper.Map(model, headers, tokens);

            return model;
        }

        public async Task<T> ReadRowAsync<T>()
            where T : class, new()
        {
            var tokens = GetNextTokensAsync();
            var model = CreateModel<T>();

            mapper.Map(model, headers, await tokens);

            return model;
        }

        private static T CreateModel<T>()
            where T : class, new()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        private async Task<string[]> GetNextTokensAsync()
        {
            var tokens = await reader.ReadLineAsync();

            return tokens.Split(config.ValueSeperator);
        }

        private string[] GetNextTokens()
        {
            return reader.ReadLine().Split(config.ValueSeperator);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    reader?.Dispose();

                    headers?.Clear();

                    if (config.ReleaseCacheOnDispose)
                    {
                        mapper?.Dispose();
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
    }
}
