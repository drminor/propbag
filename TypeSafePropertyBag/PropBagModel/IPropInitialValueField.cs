using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropInitialValueField : ICloneable
    {
        object InitialValue { get; set; }
        bool SetToDefault { get; set; }
        bool SetToEmptyString { get; set; }
        bool SetToNull { get; set; }
        bool SetToUndefined { get; set; }

        bool CreateNew { get; set; }
        string PropBagFCN { get; set; }

        Func<object> ValueCreator { get; set; }

        string GetStringValue();
    }
}