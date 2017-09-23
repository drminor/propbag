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

using System.Xaml;
using System.Windows.Markup;
using System.Windows.Data;

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
            return PropFactoryValueConverter.Convert(value, targetType, parameter, culture);
        }

        // Value is a string, we need to create a native object.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PropFactoryValueConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
