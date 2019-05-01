using MatthiWare.Csv.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MatthiWare.Csv.Core.Utils
{
    internal class Mapper : IDisposable
    {
        private static readonly Type CSV_COLUMN_ATTR = typeof(CsvColumnAttribute);

        private readonly Dictionary<Type, PropertyInfo[]> m_propertyCache = new Dictionary<Type, PropertyInfo[]>();
        private readonly Dictionary<PropertyInfo, string> m_columnCache = new Dictionary<PropertyInfo, string>();

        public void Map<T>(T model, IList<string> headers, string[] raw)
        {
            var properties = GetPropertiesCached(typeof(T));

            foreach (var prop in properties)
            {
                var rawValue = raw[GetHeaderIndex(headers, GetColumnNameCached(prop))];

                prop.SetValue(model,
                    (prop.PropertyType == typeof(string) ? rawValue : Convert.ChangeType(rawValue, prop.PropertyType)),
                    null);
            }
        }

        private int GetHeaderIndex(IList<string> self, string clmnName)
        {
            var index = self.IndexOf(clmnName);

            if (index == -1)
                throw new InvalidOperationException("Column name not specified");

            return index;
        }

        private string GetColumnNameCached(PropertyInfo property)
        {
            if (m_columnCache.TryGetValue(property, out string value))
                return value;

            value = GetColumnName(property);

            m_columnCache.Add(property, value);

            return value;
        }

        private string GetColumnName(PropertyInfo property)
            => ((CsvColumnAttribute)property.GetCustomAttributes(CSV_COLUMN_ATTR, false).First()).ColumnName;

        private PropertyInfo[] GetPropertiesCached(Type type)
        {
            if (m_propertyCache.TryGetValue(type, out PropertyInfo[] value))
                return value;

            value = GetProperties(type).ToArray();

            m_propertyCache.Add(type, value);

            return value;
        }
        private IEnumerable<PropertyInfo> GetProperties(Type type)
            => type.GetTypeInfo()
            .DeclaredProperties
            .Where(prop => prop.GetCustomAttributes(CSV_COLUMN_ATTR, false).Any());

        public void Dispose()
        {
            m_columnCache.Clear();
            m_propertyCache.Clear();
        }
    }
}
