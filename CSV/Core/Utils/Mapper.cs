using MatthiWare.Csv.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MatthiWare.Csv.Core.Utils
{
    internal class Mapper : IDisposable
    {
        private static readonly Type csvColumnAttribute = typeof(CsvColumnAttribute);

        private readonly Dictionary<Type, PropertyInfo[]> propertyCache = new();
        private readonly Dictionary<PropertyInfo, string> columnCache = new();

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
            {
                throw new InvalidOperationException("Column name not specified");
            }

            return index;
        }

        private string GetColumnNameCached(PropertyInfo property)
        {
            if (columnCache.TryGetValue(property, out string value))
            {
                return value;
            }

            value = GetColumnName(property);

            columnCache.Add(property, value);

            return value;
        }

        private string GetColumnName(PropertyInfo property)
            => ((CsvColumnAttribute)property.GetCustomAttributes(csvColumnAttribute, false).First()).ColumnName;

        private PropertyInfo[] GetPropertiesCached(Type type)
        {
            if (propertyCache.TryGetValue(type, out PropertyInfo[] value))
            {
                return value;
            }

            value = GetProperties(type).ToArray();

            propertyCache.Add(type, value);

            return value;
        }
        private IEnumerable<PropertyInfo> GetProperties(Type type)
            => type.GetTypeInfo()
            .DeclaredProperties
            .Where(prop => prop.GetCustomAttributes(csvColumnAttribute, false).Any());

        public void Dispose()
        {
            columnCache.Clear();
            propertyCache.Clear();
        }
    }
}
