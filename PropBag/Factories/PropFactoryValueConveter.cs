﻿using System;
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

using System.Windows;
using System.Windows.Data;
using DRM.TypeSafePropertyBag;

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

            // The parameter, if only specifying one type, is specifying the type
            // of the native (i.e., source) object.
            TwoTypes tt = GetFromParam(parameter, typeof(string));

            if (tt.IsEmpty) throw new InvalidOperationException("Type information was not available.");

            if (targetType != tt.SourceType)
            {
                StringFromTDelegate del = GetTheStringFromTDelegate(tt.SourceType, tt.DestType);
                return del(value);
            }
            return value;
        }

        // Value is a string, we need to create a native object.
        public static object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && !targetType.IsValueType) return null;

            // The parameter, if only specifying one type, is specifying the type
            // of the native (i.e., source) object.

            System.Diagnostics.Debug.Assert(value == null || typeof(string).IsAssignableFrom(value.GetType()), $"PropFactory expected string input on convert back, but type was {value.GetType()}.");
            TwoTypes tt = GetFromParam(parameter, typeof(string));

            if (tt.IsEmpty) throw new InvalidOperationException("Type information was not available.");

            if (value.GetType() != tt.SourceType)
            {
                TFromStringDelegate del = GetTheTFromStringDelegate(tt.SourceType);
                string s = value as string;
                return del(s);
            }

            return value;
        }

        private static TwoTypes GetFromParam(object parameter, Type destinationType = null)
        {
            if (parameter == null)
            {
                return TwoTypes.Empty;
            }
            else if(parameter is TwoTypes)
            { 
                return (TwoTypes)parameter;
            }
            else if(parameter is Type && destinationType != null)
            {
                return new TwoTypes((Type)parameter, destinationType);
            }
            else if(parameter is IPropGen && destinationType != null)
            {
                return new TwoTypes(((IPropGen)parameter).Type, destinationType);
            }
            else
            {
                return TwoTypes.Empty;
            }

        }


        #region Helper Methods for the Generic Method Templates

        // Delegate declarations.
        private delegate object TFromStringDelegate(string strVal);

        private delegate string StringFromTDelegate(object value);

        // TODO: Cache these.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType">The native or DataContext type from a string representation. The GenericType parameter: T is this type. </param>
        /// <returns></returns>
        private static TFromStringDelegate GetTheTFromStringDelegate(Type sourceType)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("A TFromString delegate is being created for type: {0}", sourceType.ToString()));

            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetTfromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(sourceType);
            TFromStringDelegate result = (TFromStringDelegate)Delegate.CreateDelegate(typeof(TFromStringDelegate), methInfoGetProp);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyType">The Type of the property on the view.</param>
        /// <param name="sourceType">The Type of value in the DataContext, The GenericType parameter: T is this type.</param>
        /// <returns></returns>
        private static StringFromTDelegate GetTheStringFromTDelegate(Type sourceType, Type propertyType)
        {
            // IsConvert is performed when going from native (or T) to string.
            TypeDescBasedTConverterKey key = new TypeDescBasedTConverterKey(sourceType, propertyType, isConvert: true);

            StringFromTDelegate result = LookupTypeDescripterConverter(key);

            System.Diagnostics.Debug.WriteLine(
                string.Format("A StringFromT delegate is being requested for type: {0} and was {1}",
                    propertyType.ToString(), result == null ? "not found." : "found."));

            if (result != null) return result;

            // NOTE: Changed this from PropertyType to SourceType on 10/2/2017 at 11:30 pm
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetStringFromT", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(sourceType);
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
