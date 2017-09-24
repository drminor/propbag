using System.Diagnostics;
using System.Linq.Expressions;

#if NET45
using AgileObjects.ReadableExpressions;
#endif

namespace AutoMapper
{
    public static class DebugHelpers
    {
        public static void LogExpression(Expression a, string name)
        {
            string readable = "Not Supported";
#if NET45
            readable = a.ToReadableString();
#endif
            Debug.WriteLine(string.Format("Start {0}:", name));
            Debug.WriteLine(string.Format("{0}", readable));
            Debug.WriteLine(string.Format("End {0}:", name));
        }
    }
}
