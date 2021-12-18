using System;

namespace MatthiWare.Csv.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public int ColumnIndex { get; private set; }

        public CsvColumnAttribute(string columName)
        {
            ColumnName = columName;
        }

        public CsvColumnAttribute(int columnIndex)
        {
            ColumnIndex = columnIndex;
        }
    }
}
