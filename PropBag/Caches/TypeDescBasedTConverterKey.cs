using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Reflection;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.Caches
{
    public class TypeDescBasedTConverterKey
    {
        static TDBasedTConverterEquComparer singleInstanceOfOurComparer = null;

        public Type SourceType { get; private set; }
        public Type TargetType { get; private set; }
        public bool IsConvert { get; private set; } // True if this for the Convert method, otherwise False for the ConvertBack method.

        // Disalow use of default constructor.
        private TypeDescBasedTConverterKey() { } 

        public TypeDescBasedTConverterKey(Type sourceType, Type targetType, bool isConvert) 
        {
            SourceType = sourceType;
            TargetType = targetType;
            IsConvert = isConvert;
        }

        /// <summary>
        /// Returns a new instance of a Equality Comparer for this type.
        /// </summary>
        /// <returns>The single app-domain-wide Equality Comparer for keys of this type.</returns>
        static public IEqualityComparer<TypeDescBasedTConverterKey> GetEquComparer()
        {
            if (singleInstanceOfOurComparer == null)
            {
                singleInstanceOfOurComparer = new TDBasedTConverterEquComparer();
            }
            return singleInstanceOfOurComparer;
        }
    }

    /// <summary>
    /// IEqualityComparer that can be used for a set of delegates that share a common generator source.
    /// An example of a generator source being TypeDescriptor.GetTypeConverter.
    /// If Delegates from multiple generator sources will be held in a single collection then
    /// an additional field should be added to distingish between delgates from diferent sources.
    /// </summary>
    public class TDBasedTConverterEquComparer : IEqualityComparer<TypeDescBasedTConverterKey>
    {
        public bool Equals(TypeDescBasedTConverterKey x, TypeDescBasedTConverterKey y)
        {
            return (x.SourceType.Equals(y.SourceType) && x.TargetType.Equals(y.TargetType) && x.IsConvert == y.IsConvert);
        }

        public int GetHashCode(TypeDescBasedTConverterKey obj)
        {
            return GenerateHash.CustomHash(obj.SourceType.GetHashCode(), obj.TargetType.GetHashCode(), obj.IsConvert ? 98000 : 99001);
        }
    }
}
