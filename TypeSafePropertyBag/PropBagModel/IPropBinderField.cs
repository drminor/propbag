using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropBinderField : ICloneable
    {
        string Path { get; set; }
    }
}