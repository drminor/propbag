using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropInitialValueField
    {
        string InitialValue { get; set; }
        bool SetToDefault { get; set; }
        bool SetToEmptyString { get; set; }
        bool SetToNull { get; set; }
        bool SetToUndefined { get; set; }
        Func<object> ValueCreator { get; set; }

        string GetStringValue();
    }
}