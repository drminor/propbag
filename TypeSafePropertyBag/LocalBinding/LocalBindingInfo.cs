using System.Globalization;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public struct LocalBindingInfo
    {
        public LocalBindingInfo(LocalPropertyPath propertyPath) : this()
        {
            PropertyPath = propertyPath;
        }

        public LocalBindingInfo(LocalPropertyPath propertyPath, LocalBindingMode mode) : this(propertyPath)
        {
            Mode = mode;
        }

        public LocalPropertyPath PropertyPath { get; set; }
        public LocalBindingMode Mode { get; set; }

        public string BindingGroupName { get; set; }

        //ILocalValueConverter _converter;
        public ILocalValueConverter<T> GetConverter<T>()
        {
            return null;
        }

        private void SetConverter<T>()
        {

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


        //public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

        public bool ValidatesOnDataErrors { get; set; }
        public bool ValidatesOnExceptions { get; set; }
        public bool ValidatesOnNotifyDataErrors { get; set; }

        public int Delay { get; set; }

    }

}
