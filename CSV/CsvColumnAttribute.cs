using System;

namespace MatthiWare.Csv
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvColumnAttribute : Attribute
    {
        private readonly string m_columnName;
        private readonly int m_columnIndex;

        public string ColumnName => m_columnName;
        public int ColumnIndex => m_columnIndex;

        public CsvColumnAttribute(string columName)
        {
            m_columnName = columName;
        }

        public CsvColumnAttribute(int columnIndex)
        {
            m_columnIndex = columnIndex;
        }
    }
}
