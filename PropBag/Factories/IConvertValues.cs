using System;
using System.Globalization;

namespace DRM.PropBag
{
    /// <summary>
    /// This allows the implementation of classes that support the IPropFactory interface
    /// to provide only those features defined by this interface, while using the 
    /// default implementation provided by the AbstractPropFactory.
    /// 
    /// This is similar to the IValueConverter interface.
    /// </summary>
    public interface IConvertValues
    {
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        object GetDefaultValue(Type propertyType, string propertyName = null);

        T GetDefaultValue<T>(string propertyName = null);
    }
}
