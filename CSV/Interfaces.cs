namespace MatthiWare.Csv
{

    public interface ICsvDataRow
    {
        dynamic DynamicContent { get; }

        string[] Values { get; }

        string this[string name] { get; }

        string this[int index] { get; }
    }

}
