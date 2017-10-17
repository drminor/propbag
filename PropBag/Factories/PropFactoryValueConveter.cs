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

using System.Windows;
using System.Windows.Data;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag
{
    public class PropFactoryValueConverter : IConvertValues
    {
        //static private Type GMT_TYPE = typeof(GenericMethodTemplatesPropConv);

        TypeDescBasedTConverterCache _converter;

        public PropFactoryValueConverter()
        {
            _converter = DelegateCacheProvider.TypeDescBasedTConverterCache;
        }

        // Value is native object, we need to return a targetType (hopefully a string at this point.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                if (value == null) return string.Empty;
            }

            System.Diagnostics.Debug.Assert(value == null || targetType.IsAssignableFrom(typeof(string)), $"PropFactory expected target type to be string, but was type: {targetType}.");


            // The parameter, if only specifying one type, is specifying the type
            // of the native (i.e., source) object.
            TwoTypes tt = TwoTypes.FromMkUpExtParam(parameter, typeof(string));

            if (tt.IsEmpty) throw new InvalidOperationException("Type information was not available.");

            if (targetType != tt.SourceType)
            {
                StringFromTDelegate del = _converter.GetTheStringFromTDelegate(tt.SourceType, tt.DestType);
                return del(value);
            }
            return value;
        }

        // Value is a string, we need to create a native object.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && !targetType.IsValueType) return null;

            // The parameter, if only specifying one type, is specifying the type
            // of the native (i.e., source) object.

            System.Diagnostics.Debug.Assert(value == null || typeof(string).IsAssignableFrom(value.GetType()), $"PropFactory expected string input on convert back, but type was {value.GetType()}.");
            TwoTypes tt = TwoTypes.FromMkUpExtParam(parameter, typeof(string));

            if (tt.IsEmpty) throw new InvalidOperationException("Type information was not available.");

            if (value.GetType() != tt.SourceType)
            {
                TFromStringDelegate del = _converter.GetTheTFromStringDelegate(tt.SourceType, tt.DestType);
                string s = value as string;
                return del(s);
            }

            return value;
        }

        public T GetDefaultValue<T>(string propertyName = null)
        {
            return default(T);
        }

        public object GetDefaultValue(Type propertyType, string propertyName = null)
        {
            if (propertyType == null)
            {
                throw new InvalidOperationException($"Cannot manufacture a default value if the type is specified as null for property: {propertyName}.");
            }

            if (propertyType == typeof(string))
                return null;

            return Activator.CreateInstance(propertyType);
        }

        //private TwoTypes GetFromParam(object parameter, Type destinationType = null)
        //{
        //    if (parameter == null)
        //    {
        //        return TwoTypes.Empty;
        //    }
        //    else if(parameter is TwoTypes)
        //    { 
        //        return (TwoTypes)parameter;
        //    }
        //    else if(parameter is Type && destinationType != null)
        //    {
        //        return new TwoTypes((Type)parameter, destinationType);
        //    }
        //    else if(parameter is IPropGen && destinationType != null)
        //    {
        //        return new TwoTypes(((IPropGen)parameter).Type, destinationType);
        //    }
        //    else
        //    {
        //        return TwoTypes.Empty;
        //    }
        //}

    }

}
