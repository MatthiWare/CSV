namespace MatthiWare.Csv
{
    public class CsvConfig
    {
        public bool FirstLineIsHeader { get; set; } = true;

        public bool GenerateDefaultHeadersIfNotFound { get; set; } = true;

        public char ValueSeperator { get; set; } = ',';

        public bool IsStreamOwner { get; set; } = true;

        public bool ReleaseCacheOnDispose { get; set; } = true;
    }
}
