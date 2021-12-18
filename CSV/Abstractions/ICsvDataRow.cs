using System.Collections.Generic;

namespace MatthiWare.Csv.Abstractions
{

    public interface ICsvDataRow
    {
        IReadOnlyList<string> Values { get; }

        string this[string name] { get; }

        string this[int index] { get; }
    }

}
