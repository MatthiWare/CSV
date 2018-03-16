namespace MatthiWare.Csv
{

    public interface ICsvDataRow
    {
        bool IsHeaderRow { get; }

        dynamic DynamicContent { get; }

        string[] Headers { get; }

        string[] Values { get; }

        string this[string name] { get; }

        string this[int index] { get; }
    }

}
