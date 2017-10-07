using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace DRM.PropBag.ControlsWPF
{
    public class PropValueConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        // Value is native object, we need to return a targetType (hopefully a string at this point.)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine("Calling Convert from PropValueConverter.");
            try
            {
                object result = PropFactoryValueConverter.Convert(value, targetType, parameter, culture);
                return result;
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        // Value is a string, we need to create a native object.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.WriteLine("Calling ConvertBack from PropValueConverter.");

            try
            {
                object result = PropFactoryValueConverter.ConvertBack(value, targetType, parameter, culture);
                return result;
            }
            catch
            {
                return Binding.DoNothing;
            }

        }
    }
}
