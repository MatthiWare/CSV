using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MatthiWare.Csv.Abstractions
{
    public interface ICsvReader : IDisposable
    {
        IReadOnlyCollection<string> Headers { get; }
        CsvConfig Config { get; }
        bool HasData { get; }
        IEnumerable<ICsvDataRow> ReadRows();
        IEnumerable<Task<ICsvDataRow>> ReadRowsAsync();
        ICsvDataRow ReadRow();
        Task<ICsvDataRow> ReadRowAsync();
        IEnumerable<T> ReadRows<T>() where T : class, new();
        IEnumerable<Task<T>> ReadRowsAsync<T>() where T : class, new();
        T ReadRow<T>() where T : class, new();
        Task<T> ReadRowAsync<T>() where T : class, new();
    }
}
