using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.ComponentModel.Design.Serialization;

using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;


using System.Windows.Data;

namespace DRM.PropBag
{
    public class PropFactoryValueConverter 
    {
        static private Type GMT_TYPE = typeof(GenericMethodTemplatesPropConv);

        // Value is native object, we need to return a targetType (hopefully a string at this point.)
        public static object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                if (value == null) return string.Empty;
            }

            TwoTypes tt = GetFromParam(parameter);

            if (!tt.IsEmpty)
            {
                if (targetType == tt.SourceType)
                {
                    StringFromTDelegate del = GetTheStringFromTDelegate(tt.DestType, tt.SourceType);
                    return del(value);
                }
            }

            return null;
        }

        // Value is a string, we need to create a native object.
        public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && !targetType.IsValueType) return null;
            
            TwoTypes tt = GetFromParam(parameter);
            if (!tt.IsEmpty)
            {
                if (value.GetType() == tt.SourceType)
                {
                    TFromStringDelegate del = GetTheTFromStringDelegate(tt.DestType);
                    return del((string)value);
                }
            }

            return null;
        }

        private static TwoTypes GetFromParam(object parameter)
        {
            if (parameter == null)
            {
                return TwoTypes.Empty;
            }

            if (parameter is TwoTypes)
            {
                return (TwoTypes)parameter;
            }

            // Assume string (TODO: Check This.
            Type sourceType = typeof(string);

            if (parameter is Type)
            {
                return new TwoTypes(sourceType, (Type)parameter);
            }
            else
            {
                if (parameter is IPropGen)
                {
                    return new TwoTypes(sourceType, ((IPropGen)parameter).Type);
                }
            }
            return TwoTypes.Empty;
        }


        #region Helper Methods for the Generic Method Templates

        // Delegate declarations.
        private delegate object TFromStringDelegate(string strVal);

        private delegate string StringFromTDelegate(object value);

        private static TFromStringDelegate GetTheTFromStringDelegate(Type propertyType)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("A TFromString delegate is being created for type: {0}", propertyType.ToString()));

            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetTfromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            TFromStringDelegate result = (TFromStringDelegate)Delegate.CreateDelegate(typeof(TFromStringDelegate), methInfoGetProp);

            return result;
        }

        private static StringFromTDelegate GetTheStringFromTDelegate(Type propertyType, Type sourceType)
        {
            TypeDescBasedTConverterKey key = new TypeDescBasedTConverterKey(sourceType, propertyType, isConvert: true);

            StringFromTDelegate result = LookupTypeDescripterConverter(key);

            System.Diagnostics.Debug.WriteLine(
                string.Format("A StringFromT delegate is being requested for type: {0} and was {1}",
                    propertyType.ToString(), result == null ? "not found." : "found."));

            if (result != null) return result;

            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetStringFromT", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            result = (StringFromTDelegate)Delegate.CreateDelegate(typeof(StringFromTDelegate), methInfoGetProp);

            DelegateCacheProvider.TypeDescBasedTConverterCache.Add(key, result);

            return result;
        }

        private static StringFromTDelegate LookupTypeDescripterConverter(TypeDescBasedTConverterKey key)
        {
            if (DelegateCacheProvider.TypeDescBasedTConverterCache.ContainsKey(key))
            {
                return (StringFromTDelegate)DelegateCacheProvider.TypeDescBasedTConverterCache[key];
            }

            return null;
        }

        #endregion
    }

    #region Generic Method Templates

    static class GenericMethodTemplatesPropConv
    {
        private static object GetTfromString<T>(string strVal)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
            return (T)(tc.ConvertFromString(strVal));
        }

        private static string GetStringFromT<T>(object value)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
            return tc.ConvertToInvariantString((T)value);
        }
    }

    #endregion
}
