using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    // TODO: Rename to SimpleTypeConverterCache : ITypeConverterCache
    public class TypeDescBasedTConverterCache : ITypeDescBasedTConverterCache
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
#if DEBUG
            //bool inCache = _converters.ContainsKey(key);
            Delegate result = _converters.GetOrAdd(key);
            //System.Diagnostics.Debug.WriteLine(
                //string.Format("A StringFromT delegate is being requested for type: {0} and was {1}",
                //    propertyType.ToString(), inCache ? "found." : "not found."));
#else
            Delegate result = _converters.GetOrAdd(key);
#endif
            if (result == null) return null;
            return (StringFromTDelegate)result;
        }

        public TFromStringDelegate GetTheTFromStringDelegate(Type sourceType, Type propertyType)
        {
            TypeDescBasedTConverterKey key = new TypeDescBasedTConverterKey(sourceType, propertyType, isConvert: false);

#if DEBUG
            //bool inCache = _converters.ContainsKey(key);
            Delegate result = _converters.GetOrAdd(key);
            //System.Diagnostics.Debug.WriteLine(
            //    string.Format("A TFromString delegate is being requested for type: {0} and was {1}",
            //        sourceType.ToString(), inCache ? "found." : "not found."));
#else
            Delegate result = _converters.GetOrAdd(key);
#endif
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

        // TODO: Consider using instance methods instead of static methods. This should save a few hundred nano seconds off of the time required to make each call.
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

            return result;
        }

        #endregion
    }

    #region Generic Method Templates

    // TODO: Consider using the TypeChanger class instead of using the TypeDescriptor 'infrastructure'.
    // or perhaps using TypeDescriptor first and then ChangeType<T> class if the TypeDescriptor.GetConverter method fails.
    static class GenericMethodTemplatesPropConv
    {
        private static string GetStringFromT<T>(object value)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
            return tc.ConvertToInvariantString((T)value);

            //string result = TypeChanger.ChangeType<string>(value);
            //return result;
        }

        private static object GetTfromString<T>(string strVal)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
            return (T)(tc.ConvertFromString(strVal));

            //T result = TypeChanger.ChangeType<T>(strVal);
            //return result;
        }
    }

    #endregion
}



