using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace MatthiWare.Csv.Core
{
    internal class CsvDataRow : ICsvDataRow
    {
        private readonly IReadOnlyList<CsvDataItem> data;
        private readonly Lazy<dynamic> lazyDynamicModelResolver;

        public CsvDataRow(IReadOnlyList<CsvDataItem> data)
        {
            this.data = data;
            lazyDynamicModelResolver = new Lazy<dynamic>(ToDynamicModel);
        }

        public string this[string name] => data.First(kvp => kvp.Key.Equals(name)).Value;

        public string this[int index] => data[index].Value;

        public dynamic Row => lazyDynamicModelResolver.Value;

        public IReadOnlyList<string> Values => data.Select(kvp => kvp.Value).ToArray();

        private dynamic ToDynamicModel()
        {
            var expando = new ExpandoObject();
            var dict = (IDictionary<string, object>)expando;

            foreach (var item in data)
                dict.Add(item.Key, item.Value);

            return expando;
        }
    }
}
