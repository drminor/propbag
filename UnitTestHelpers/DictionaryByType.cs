using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestHelpers
{

    /// <summary>
    /// Map from types to instances of those types, e.g. int to 10 and
    /// string to “hi” within the same dictionary. This cannot be done
    /// without casting (and boxing for value types) as .NET cannot
    /// represent this relationship with generics in their current form.
    /// This class encapsulates the nastiness in a single place.
    /// </summary>
    /// 
    public class DictionaryByType
    {
        public readonly IDictionary<Type, object> dictionary = new Dictionary<Type, object>();
        /// <summary>
        /// Maps the specified type argument to the given value. If
        /// the type argument already has a value within the dictionary,
        /// ArgumentException is thrown.
        /// </summary>
        public void Add<T>(TestValues<T> vals)
        {
            dictionary.Add(typeof(T), vals);
        }

        /// <summary>
        /// Maps the specified type argument to the given value. If
        /// the type argument already has a value within the dictionary, it
        /// is overwritten.
        /// </summary>
        public void Put<T>(TestValues<T> vals)
        {
            dictionary[typeof(T)] = vals;
        }

        /// <summary>
        /// Attempts to fetch a value from the dictionary, throwing a
        /// KeyNotFoundException if the specified type argument has no
        /// entry in the dictionary.
        /// </summary>
        public TestValues<T> Get<T>()
        {
            return (TestValues<T>)dictionary[typeof(T)];
        }

        /// <summary>
        /// Attempts to fetch a value from the dictionary, returning false and
        /// setting the output parameter to the default value for T if it
        /// fails, or returning true and setting the output parameter to the
        /// fetched value if it succeeds.
        /// </summary>
        public bool TryGet<T>(out TestValues<T> vals)
        {
            object tmp;
            if (dictionary.TryGetValue(typeof(T), out tmp))
            {
                vals = (TestValues<T>)tmp;
                return true;
            }
            vals = null;
            return false;
        }
    }
}
