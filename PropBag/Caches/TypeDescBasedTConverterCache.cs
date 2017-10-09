using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.Caches
{
    // Delegate declarations. TODO: Move to dedicated file.
    public delegate object TFromStringDelegate(string strVal);
    public delegate string StringFromTDelegate(object value);

    public class TypeDescBasedTConverterCache
    {
        private LockingConcurrentDictionary<TypeDescBasedTConverterKey, Delegate> _converters;

        public TypeDescBasedTConverterCache()
        {
            _converters = new LockingConcurrentDictionary<TypeDescBasedTConverterKey, Delegate>(
                CreateConverter, TypeDescBasedTConverterKey.GetEquComparer());
        }

        public StringFromTDelegate GetTheStringFromTDelegate(Type sourceType, Type propertyType)
        {
            TypeDescBasedTConverterKey key = new TypeDescBasedTConverterKey(sourceType, propertyType, isConvert: true);
            Delegate result = _converters.GetOrAdd(key);

            if (result == null) return null;
            return (StringFromTDelegate)result;
        }

        public TFromStringDelegate GetTheTFromStringDelegate(Type sourceType, Type propertyType)
        {
            TypeDescBasedTConverterKey key = new TypeDescBasedTConverterKey(sourceType, propertyType, isConvert: false);
            Delegate result = _converters.GetOrAdd(key);

            if (result == null) return null;
            return (TFromStringDelegate)result;
        }

        private Delegate CreateConverter(TypeDescBasedTConverterKey key)
        {
            if (key.IsConvert)
            {
                return GetTheStringFromTDelegateInt(key.SourceType, key.TargetType);
            }
            else
            {
                return GetTheTFromStringDelegateInt(key.SourceType, key.TargetType);
            }
        }

        #region Internal Delegate Builders

        static private Type GMT_TYPE = typeof(GenericMethodTemplatesPropConv);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyType">The Type of the property on the view.</param>
        /// <param name="sourceType">The Type of value in the DataContext, The GenericType parameter: T is this type.</param>
        /// <returns></returns>
        private StringFromTDelegate GetTheStringFromTDelegateInt(Type sourceType, Type propertyType)
        {
            // IsConvert is performed when going from native (or T) to string.

            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetStringFromT", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(sourceType);
            StringFromTDelegate result = (StringFromTDelegate)Delegate.CreateDelegate(typeof(StringFromTDelegate), methInfoGetProp);

            System.Diagnostics.Debug.WriteLine(
                string.Format("A StringFromT delegate is being requested for type: {0} and was {1}",
                    propertyType.ToString(), result == null ? "not found." : "found."));

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType">The native or DataContext type from a string representation. The GenericType parameter: T is this type. </param>
        /// <returns></returns>
        private TFromStringDelegate GetTheTFromStringDelegateInt(Type sourceType, Type propertyType)
        {
            // IsConvert is performed when going from native (or T) to string.

            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetTfromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(sourceType);
            TFromStringDelegate result = (TFromStringDelegate)Delegate.CreateDelegate(typeof(TFromStringDelegate), methInfoGetProp);

            System.Diagnostics.Debug.WriteLine(
                string.Format("A TFromString delegate is being requested for type: {0} and was {1}",
                    sourceType.ToString(), result == null ? "not found." : "found."));

            return result;
        }

        #endregion
    }

    #region Generic Method Templates

    static class GenericMethodTemplatesPropConv
    {
        private static string GetStringFromT<T>(object value)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
            return tc.ConvertToInvariantString((T)value);
        }

        private static object GetTfromString<T>(string strVal)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
            return (T)(tc.ConvertFromString(strVal));
        }
    }

    #endregion
}



