using System;

namespace DRM.TypeSafePropertyBag
{
    public interface ITypeSafePropBag
    {
        Type GetTypeOfProperty(string propertyName);

        object GetValWithType(string propertyName, Type propertyType);

        bool SetValWithType(string propertyName, Type propertyType, object value);
    }
}
