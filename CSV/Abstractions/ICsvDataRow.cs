using System.Collections.Generic;

namespace MatthiWare.Csv
{

    public interface ICsvDataRow
    {
        dynamic Row { get; }

        IReadOnlyList<string> Values { get; }

        string this[string name] { get; }

        string this[int index] { get; }
    }

}
