namespace MatthiWare.Csv
{

    public interface ICsvDataRow
    {
        string[] Values { get; }

        string this[string name] { get; }

        string this[int index] { get; }
    }

}
