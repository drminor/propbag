using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.XMLModel
{
    public class PropModelStaticText
    {

        static string[] cache = new string[1];

        static public string GetGetDelegateMethodText()
        {
            if(cache[0] == null)
            {
                //List<string> result = new List<string>();
                //result.Add("/// <summary>");
                //result.Add("/// If the delegate exists, the original name is returned,");
                //result.Add("/// otherwise null is returned.");
                //result.Add("/// </summary>");
                //result.Add("/// <param name=\"methodName\">Some public or non-public instance method in this class.</param>");
                //result.Add("/// <returns>The name, unchanged, if the method exists, otherwise null.</returns>");
                //result.Add("private Action<T, T> GetDelegate<T>(string methodName)");
                //result.Add("{");
                //result.Add("    Type pp = this.GetType();");
                //result.Add("    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);");
                //result.Add("");
                //result.Add("    if (mi == null) return null;");
                //result.Add("");
                //result.Add("    Action<T, T> result = (Action<T, T>)mi.CreateDelegate(typeof(Action<T, T>), this);");
                //result.Add("");
                //result.Add("    return result;");
                //result.Add("}");

                List<string> result = new List<string>();
                result.Add("/// <summary>");
                result.Add("/// If the delegate exists, the original name is returned,");
                result.Add("/// otherwise null is returned.");
                result.Add("/// </summary>");
                result.Add("/// <param name=\"methodName\">Some public or non-public instance method in this class.</param>");
                result.Add("/// <returns>The name, unchanged, if the method exists, otherwise null.</returns>");
                result.Add("EventHandler<PropertyChangedWithTValsEventArgs<T>> GetDelegate<T>(string methodName)");
                result.Add("{");
                result.Add("    Type pp = this.GetType();");
                result.Add("    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);");
                result.Add("");
                result.Add("    if (mi == null) return null;");
                result.Add("");
                result.Add("    EventHandler<PropertyChangedWithTValsEventArgs<T>> result = (EventHandler<PropertyChangedWithTValsEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PropertyChangedWithTValsEventArgs<T>>), this);");
                result.Add("");
                result.Add("    return result;");
                result.Add("}");



                cache[0] = TextFromList(result, 2);

            }
            return cache[0];
        }

                // --- NEW --- //
        ///// <summary>
        ///// If the delegate exists, the original name is returned,
        ///// otherwise null is returned.
        ///// </summary>
        ///// <param name="methodName">Some public or non-public instance method in this class.</param>
        ///// <returns>The name, unchanged, if the method exists, otherwise null.</returns>
        //private EventHandler<PropertyChangedWithTValsEventArgs<T>> GetDelegate<T>(string methodName)
        //{
        //    Type pp = this.GetType();
        //    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //    if (mi == null) return null;

        //    EventHandler<PropertyChangedWithTValsEventArgs<T>> result = (EventHandler<PropertyChangedWithTValsEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PropertyChangedWithTValsEventArgs<T>>), this);

        //    return result;
        //}

                // --- OLD --- //
        ///// <summary>
        ///// If the delegate exists, the original name is returned,
        ///// otherwise null is returned.
        ///// </summary>
        ///// <param name="methodName">Some public or non-public instance method in this class.</param>
        ///// <returns>The name, unchanged, if the method exists, otherwise null.</returns>
        //private Action<T, T> GetDelegate<T>(string methodName)
        //{
        //    Type pp = this.GetType();
        //    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //    if (mi == null) return null;

        //    Action<T, T> result = (Action<T, T>)mi.CreateDelegate(typeof(Action<T, T>), this);

        //    return result;
        //}

        static string TextFromList(List<string> x, int indentAmount)
        {
            StringBuilder sb = new StringBuilder();

            string prefix = new String('\t', indentAmount);

            foreach (string s in x)
            {
                sb.Append(prefix).AppendLine(s);
            }

            return sb.ToString();
        }


    }
}
