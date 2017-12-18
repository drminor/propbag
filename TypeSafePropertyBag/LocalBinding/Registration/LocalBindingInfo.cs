using System;
using System.Collections.Generic;
using System.Globalization;

namespace DRM.TypeSafePropertyBag
{
    public struct LocalBindingInfo : IEquatable<LocalBindingInfo>
    {
        public LocalBindingInfo(LocalPropertyPath propertyPath) : this()
        {
            PropertyPath = propertyPath;
            Mode = LocalBindingMode.OneWay;
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

        public override bool Equals(object obj)
        {
            return obj is LocalBindingInfo && Equals((LocalBindingInfo)obj);
        }

        public bool Equals(LocalBindingInfo other)
        {
            return EqualityComparer<LocalPropertyPath>.Default.Equals(PropertyPath, other.PropertyPath);
        }

        public override int GetHashCode()
        {
            int hashCode = PropertyPath.GetHashCode();
            return hashCode;
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

        public static bool operator ==(LocalBindingInfo info1, LocalBindingInfo info2)
        {
            return info1.Equals(info2);
        }

        public static bool operator !=(LocalBindingInfo info1, LocalBindingInfo info2)
        {
            return !(info1 == info2);
        }
    }

}
