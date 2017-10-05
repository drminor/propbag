using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class MyMultiValueConverter : IMultiValueConverter
    {

        public MultiBinding MultiBinding { get; }
        BindingMode _mode;

        public MyMultiValueConverter(BindingMode mode = BindingMode.Default)
        {
            _mode = mode;
            MultiBinding = new MultiBinding
            {
                Converter = this
            };

            // binding to evaluate data context
            Add(new Binding { Mode = BindingMode.OneWay });

            // binding to evaluate target element
            Add(new Binding
            {
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            });
        }

        public BindingBase this[int index]
        {
            get
            {
                return MultiBinding.Bindings[index];
            }
            set
            {
                MultiBinding.Bindings[index] = value;
            }
        }

        public void Add(BindingBase binding)
        {
            MultiBinding.Bindings.Add(binding);
        }

        public MultiBindingExpression GetMultiBindingExpression(IServiceProvider serviceProvider)
        {
            return MultiBinding.ProvideValue(serviceProvider) as MultiBindingExpression;
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DependencyObject targetObject = values[1] as DependencyObject;

            if (targetObject == null)
            {
                return null;
            }

            object value = values[values.Length - 1];

            return value;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = new object[targetTypes.Length];

            for (int i = 0; i < result.Length - 1; i++)
            {
                result[i] = Binding.DoNothing;
            }
            result[result.Length - 1] = value;

            return result;
        }

    }
}
