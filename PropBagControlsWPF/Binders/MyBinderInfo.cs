using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public struct MyBindingInfo
    {
        public MyBindingInfo(PropertyPath propertyPath) : this()
        {
            PropertyPath = propertyPath;
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

        public IValueConverter Converter { get; set; }
        public CultureInfo ConverterCulture { get; set; }
        public object ConverterParameter { get; set; }

        public object FallBackVallue { get; set; }
        public bool IsAsync { get; set; }

        public bool NotifyOnSourceUpdated { get; set; }
        public bool NotifyOnTargetUpdated { get; set; }
        public bool NotifyOnValidationError { get; set; }

        public string StringFormat { get; set; }
        public object TargetNullValue { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public bool ValidatesOnDataErrors { get; set; }
        public bool ValidatesOnExceptions { get; set; }

        public string XPath { get; set; }
    }

}
