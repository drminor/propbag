using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class MyMultiValueConverter : IMultiValueConverter
    {

        private MultiBinding _multiBinding;

        //public int NumberOfSupportBindings { get; private set; }

        public BindingMode Mode
        {
            get
            {
                return _multiBinding.Mode;
            }
        }

        public MyMultiValueConverter(BindingMode mode = BindingMode.Default)
        {
            _multiBinding = new MultiBinding
            {
                Mode = mode,
                Converter = this
            };

            //// binding to evaluate data context
            //Add(new Binding
            //{
            //    Mode = BindingMode.OneWay
            //});

            //// binding to evaluate target element
            //Add(new Binding
            //{
            //    Mode = BindingMode.OneWay,
            //    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            //});

            //NumberOfSupportBindings = 2;
        }

        public BindingBase this[int index]
        {
            get
            {
                return _multiBinding.Bindings[index];
            }
            set
            {
                _multiBinding.Bindings[index] = value;
            }
        }

        public void Add(BindingBase binding)
        {
            _multiBinding.Bindings.Add(binding);
        }

        public MultiBindingExpression GetMultiBindingExpression(IServiceProvider serviceProvider)
        {
            return _multiBinding.ProvideValue(serviceProvider) as MultiBindingExpression;
        }


        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //DependencyObject targetObject = values[1] as DependencyObject;

            //if (targetObject == null)
            //{
            //    return null;
            //}

            object value = values[values.Length - 1];

            BindingBase bb = this[values.Length - 1];
            if (bb is Binding b)
            {
                if (b.Converter == null)
                {
                    // If no converter specified by the view designer
                    // then we need to convert the value.
                    if (value != null)
                    {
                        try
                        {
                            if (!targetType.IsAssignableFrom(value.GetType()))
                            {
                                TypeConverter tc = TypeDescriptor.GetConverter(value);
                                value = tc.ConvertTo(value, targetType);
                                return value;
                            }
                        }
                        catch
                        {
                            return Binding.DoNothing;
                        }
                    }
                }
            }

            return value;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] result = new object[targetTypes.Length];

            for (int i = 0; i < result.Length - 1; i++)
            {
                result[i] = Binding.DoNothing;
            }

            BindingBase bb = this[targetTypes.Length - 1];
            if (bb is Binding b)
            {
                if (b.Converter == null)
                {
                    // If no converter specified by the view designer
                    // then we need to convert the value.
                    if (value != null)
                    {
                        try
                        {
                            Type targetType = targetTypes[targetTypes.Length - 1];
                            Type sourceType = value.GetType();
                            if (!sourceType.IsAssignableFrom(targetType))
                            {
                                TypeConverter tc = TypeDescriptor.GetConverter(targetType);
                                value = tc.ConvertTo(value, targetType);
                            }
                        }
                        catch
                        {
                            value = Binding.DoNothing;
                        }
                    }
                }
            }

            result[result.Length - 1] = value;

            return result;
        }

    }
}
