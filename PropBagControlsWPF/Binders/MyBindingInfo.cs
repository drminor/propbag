using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBagControlsWPF.Binders
{
    public struct MyBindingInfo
    {
        public MyBindingInfo(PropertyPath propertyPath) : this()
        {
            PropertyPath = propertyPath;
            ConverterParameterBuilder = GetConverterParameter;
        }

        public MyBindingInfo(PropertyPath propertyPath, BindingMode mode) : this(propertyPath)
        {
            Mode = mode;
        }

        //public string Path { get; set; }
        public PropertyPath PropertyPath { get; set; }
        public BindingMode Mode { get; set; }

        public string ElementName { get; set; }
        public object Source { get; set; }
        public RelativeSource RelativeSource { get; set; }

        public string BindingGroupName { get; set; }
        public bool BindsDirectlyToSource { get; set; }

        IValueConverter _converter;
        public IValueConverter Converter
        {
            get
            {
                return _converter;
            }
            set
            {
                _converter = value;
            }
        }
        public CultureInfo ConverterCulture { get; set; }
        public object ConverterParameter { get; set; }

        public object FallBackValue { get; set; }
        public bool IsAsync { get; set; }

        public bool NotifyOnSourceUpdated { get; set; }
        public bool NotifyOnTargetUpdated { get; set; }
        public bool NotifyOnValidationError { get; set; }


        public string StringFormat { get; set; }
        public object TargetNullValue { get; set; }


        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public bool ValidatesOnDataErrors { get; set; }
        public bool ValidatesOnExceptions { get; set; }
        public bool ValidatesOnNotifyDataErrors { get; set; }

        public string XPath { get; set; }

        public int Delay { get; set; }

        /// <summary>
        /// The UpdateSourceExceptionFilter cannot be set from XAML.
        /// This property definition is probably not needed.
        /// </summary>
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter { get; set; }

        /// <summary>
        /// The VaidationRules property of Binding object cannot be set -- it readonly.
        /// This property definition is propbably not needed.
        /// </summary>
        public Collection<System.Windows.Controls.ValidationRule> ValidationRules { get; set; }

        public Func<BindingTarget, MyBindingInfo, Type, string, object> ConverterParameterBuilder { get; set; }

        private object GetConverterParameter(BindingTarget bindingTarget, MyBindingInfo bInfo, Type sourceType, string pathElement)
        {
            return bInfo.ConverterParameter;
        }
    }

}
