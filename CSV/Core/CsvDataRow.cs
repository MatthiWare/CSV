using MatthiWare.Csv.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace MatthiWare.Csv.Core
{
    internal class CsvDataRow : ICsvDataRow
    {
        private readonly IReadOnlyList<CsvDataItem> data;

        public CsvDataRow(IReadOnlyList<CsvDataItem> data)
        {
            this.data = data;
        }

        public string this[string name] => data.First(kvp => kvp.Key.Equals(name)).Value;

        public string this[int index] => data[index].Value;

        public IReadOnlyList<string> Values => data.Select(kvp => kvp.Value).ToArray();
    }
}
