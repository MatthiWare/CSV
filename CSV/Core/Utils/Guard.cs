using System;
using System.IO;

namespace MatthiWare.Csv.Core.Utils
{
    internal static class Guard
    {
        internal const string ARG_NOT_NULL = "Argument cannot be null or empty";

        public static T CheckNotNull<T>(T input, string paramName, string msg = ARG_NOT_NULL)
        {
            if (input == null)
            {
                throw new ArgumentNullException(paramName, msg);
            }

            return input;
        }

        public static string CheckNotNull(string input, string paramName, string msg = ARG_NOT_NULL)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(paramName, msg);
            }

            return input;
        }

        public static bool CheckEndOfStream<T>(T input) where T : StreamReader
            => input.Peek() == -1 || input.EndOfStream;



    }
}
