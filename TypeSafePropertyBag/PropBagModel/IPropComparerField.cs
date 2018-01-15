using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropComparerField
    {
        Delegate Comparer { get; set; }
        bool UseRefEquality { get; set; }
    }
}