using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public sealed class SelectedItemConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        object placeholder = null;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return placeholder;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && string.Equals("{NewItemPlaceholder}", value.ToString(), StringComparison.Ordinal))
            {
                placeholder = value;
                return null;
            }

            return value;
        }




    }

}
