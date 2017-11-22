using System;
using System.Globalization;

namespace DRM.TypeSafePropertyBag
{
    //public interface ITypedValueConverter<TSource, TTarget> 
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <param name="parameter"></param>
    //    /// <param name="culture"></param>
    //    /// <returns></returns>
    //    TTarget Convert(TSource value, object parameter, CultureInfo culture);

    //    TSource ConvertBack(TTarget value, object parameter, CultureInfo culture);
    //}

    public interface ILocalValueConverter<TSource>
    {
        /// <summary>
        /// Converts a value that it produced by the source before being used to fill the
        /// method arguments of a Action where the values are the old and new values and are of type: <paramref name="targetType"/>.
        /// For two-way bindings a pair of objects that implement this interface, one for each direction.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A new object of type: <paramref name="targetType"/>.</returns>
        object Convert(TSource value, Type targetType, object parameter, CultureInfo culture);
    }
}
