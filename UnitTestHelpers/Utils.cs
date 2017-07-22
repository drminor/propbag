using System;
using System.Text;


namespace UnitTestHelpers
{
    static public class Utils
    {
        static public string GetStringFromChars(params char[] c)
        {
            if (c.Length < 1) return null;
            return new string(c);
        }

    }
}
