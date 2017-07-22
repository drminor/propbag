using System;
using System.Text;


namespace PropBagLib.Tests
{
    public class UnitTestHelpers
    {
        // Makes a string which cannot possibly be interned.
        public static string GetNewString(string x)
        {
            return new StringBuilder().Append(x).ToString();
        }
    }
}
