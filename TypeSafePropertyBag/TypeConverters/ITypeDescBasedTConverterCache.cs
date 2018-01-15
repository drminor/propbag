using System;

namespace DRM.TypeSafePropertyBag
{
    public interface ITypeDescBasedTConverterCache
    {
        StringFromTDelegate GetTheStringFromTDelegate(Type sourceType, Type propertyType);
        TFromStringDelegate GetTheTFromStringDelegate(Type sourceType, Type propertyType);
    }
}