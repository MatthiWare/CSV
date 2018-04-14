using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MatthiWare.Csv
{
    internal class Mapper
    {
        private static readonly Type CSV_COLUMN_ATTR = typeof(CsvColumnAttribute);

        private readonly Dictionary<Type, PropertyInfo[]> m_propertyCache = new Dictionary<Type, PropertyInfo[]>();
        private readonly Dictionary<PropertyInfo, string> m_columnCache = new Dictionary<PropertyInfo, string>();

        public void Map<T>(T model, IDictionary<string, int> headers, string[] raw)
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

        private int GetHeaderIndex(IDictionary<string, int> self, string clmnName)
        {
            if (!self.TryGetValue(clmnName, out int value))
                throw new InvalidOperationException("Column name not specified");

            return value;
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

#if DOT_NET_STD
        private IEnumerable<PropertyInfo> GetProperties(Type type)
            => type.GetTypeInfo()
            .DeclaredProperties
            .Where(prop => prop.GetCustomAttributes(CSV_COLUMN_ATTR, false).Any());
#else
        private IEnumerable<PropertyInfo> GetProperties(Type type)
            => type
            .GetProperties()
            .Where(prop => prop.GetCustomAttributes(CSV_COLUMN_ATTR, false).Any());
#endif

        public void ReleaseCache()
        {
            m_columnCache.Clear();
            m_propertyCache.Clear();
        }

    }
}
