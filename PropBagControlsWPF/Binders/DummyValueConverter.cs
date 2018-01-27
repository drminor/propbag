using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;


namespace DRM.PropBagControlsWPF.Binders
{
    /// <summary>
    /// Maybe used to debug bindings, or to be used where a converter is required and a NOP-Conversion is
    /// suitable.
    /// </summary>
    public class DummyConverter : MarkupExtension, IValueConverter
    {
        private static DummyConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new DummyConverter();
            }
            return _converter;
        }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value; // set breakpoint here to debug your binding
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
