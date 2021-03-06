﻿using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DRM.PropBagControlsWPF
{
    public class PropFactoryValueConverter : IConvertValues
    {
        ITypeDescBasedTConverterCache _converter;

        public PropFactoryValueConverter(ITypeDescBasedTConverterCache converterCache)
        {
            _converter = converterCache ?? throw new ArgumentNullException(nameof(converterCache));
        }

        // Value is native object, we need to return a targetType (hopefully a string at this point.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string))
            {
                if (value == null) return string.Empty;
            }

            //System.Diagnostics.Debug.Assert(value == null || targetType.IsAssignableFrom(typeof(string)), $"PropFactory expected target type to be string, but was type: {targetType}.");

            // The parameter, if only specifying one type, is specifying the type
            // of the native (i.e., source) object.
            TwoTypes tt =  FromMkUpExtParam(parameter, typeof(string));

            if (tt.IsEmpty)
            {
                throw new InvalidOperationException("Type information was not available.");
            }
            else if (targetType == tt.SourceType/* || targetType == typeof(object)*/)
            {
                return value;
            }
            if(tt.DestType == typeof(object))
            {
                return value;
            }

            Type vType = value.GetType();

            if (vType == tt.DestType ||  tt.DestType.IsAssignableFrom(vType))
            {
                System.Diagnostics.Debug.WriteLine($"The Destination Type specified in the binding parameter is assignable from the run-time type of the value being converted: {vType}.");
                System.Diagnostics.Debug.Assert(tt.DestType.IsAssignableFrom(value.GetType()), $"DestinationType: { tt.DestType } is not assignable from the run-time type of the value to be converted: {value.GetType()}.");
                return value;
            }
            else if (tt.DestType != typeof(string))
            {
                throw new NotImplementedException("Converting to a type other than a string is not supported.");
            }
            else
            {
                StringFromTDelegate del = _converter.GetTheStringFromTDelegate(tt.SourceType, tt.DestType);
                return del(value);
            }
        }

        // Value is a string, we need to create a native object.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if DEBUG
            TwoTypes tt = FromMkUpExtParam(parameter, typeof(string));
            if (tt.IsEmpty) throw new InvalidOperationException("Type information was not available.");

            // Check to see if the specified type is compatible with the type of the property the converter is asking for.
            if (!targetType.IsAssignableFrom(tt.SourceType))
            {
                System.Diagnostics.Debug.WriteLine("The SourceType specified by the binding is not compatible with the TargetType on ConvertBack (PropFactoryValueConverter.");
            }

            if (value == null && !targetType.IsValueType) return null;
            //System.Diagnostics.Debug.Assert(value == null || typeof(string).IsAssignableFrom(value.GetType()), $"PropFactory expected string input on convert back, but type was {value.GetType()}.");

            //if (targetType == typeof(object)) return value;
#else
            if (value == null && !targetType.IsValueType) return null;
            if (targetType == typeof(object)) return value;

            //System.Diagnostics.Debug.Assert(value == null || typeof(string).IsAssignableFrom(value.GetType()), $"PropFactory expected string input on convert back, but type was {value.GetType()}.");

            TwoTypes tt = TwoTypes.FromMkUpExtParam(parameter, typeof(string));
            if (tt.IsEmpty) throw new InvalidOperationException("Type information was not available.");
#endif
            if (value == null)
            {
                if(targetType.IsValueType)
                {
                    return GetDefaultValue(targetType, "ConvertBackOp");
                }
            }
            else
            {
                Type sourceRunTimeType = value?.GetType();

                if (sourceRunTimeType != tt.SourceType)
                {
                    if (sourceRunTimeType != typeof(string))
                    {
                        throw new NotImplementedException("Converting from a type other than a string is not yet supported.");
                    }

                    TFromStringDelegate del = _converter.GetTheTFromStringDelegate(tt.SourceType, tt.DestType);
                    string s = value as string;
                    return del(s);
                }
            }

            return value;
        }

        public T GetDefaultValue<T>(string propertyName = null)
        {
            return default(T);
        }

        public CT GetDefaultValue<CT, T>(string propertyName = null) where CT : class, IEnumerable<T>
        {
            if(typeof(CT) == typeof(ObservableCollection<T>))
            {
                return new ObservableCollection<T>() as CT;
            }
            else
            {
                return default(CT);
            }
        }

        public object GetDefaultValue(Type propertyType, string propertyName = null)
        {
            if (propertyType == null)
            {
                throw new InvalidOperationException($"Cannot manufacture a default value if the type is specified as null for property: {propertyName}.");
            }

            if (propertyType == typeof(string))
                return null;

            return System.Activator.CreateInstance(propertyType);
        }

        private TwoTypes FromMkUpExtParam(object parameter, Type destinationType = null)
        {
            if (parameter == null)
            {
                return TwoTypes.Empty;
            }
            else if (parameter is TwoTypes)
            {
                return (TwoTypes)parameter;
            }
            else if (parameter is Type && destinationType != null)
            {
                return new TwoTypes((Type)parameter, destinationType);
            }
            else if (parameter is IPropData && destinationType != null)
            {
                return new TwoTypes(((IPropData)parameter).TypedProp.PropTemplate.Type, destinationType);
            }
            else
            {
                return TwoTypes.Empty;
            }
        }
    }
}
