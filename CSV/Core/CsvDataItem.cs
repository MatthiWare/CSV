using System.Diagnostics;

namespace MatthiWare.Csv.Core
{
    [DebuggerStepThrough]
    public class CsvDataItem
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public CsvDataItem(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
