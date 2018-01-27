using System;
using System.Collections.Generic;
using System.Globalization;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// This is similar to the System.Windows.Data.IValueConverter interface in the PresentationFramework assembly.
    /// It is used by classes that implement IPropFactory to provide initial values when creating PropItems
    /// and supplying PropItems with a function to create default values.
    /// </summary>
    public interface IConvertValues
    {
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        object GetDefaultValue(Type propertyType, string propertyName = null);

        T GetDefaultValue<T>(string propertyName = null);

        CT GetDefaultValue<CT, T>(string propertyName = null) where CT : class, IEnumerable<T>;
    }
}
