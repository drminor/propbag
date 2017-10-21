using System;

namespace DRM.TypeSafePropertyBag
{
    public interface ITypeSafePropBag
    {
        object GetValWithType(string propertyName, Type propertyType);
        bool SetValWithType(string propertyName, Type propertyType, object value);

        T GetIt<T>(string propertyName);
        bool SetIt<T>(T value, string propertyName);

        Type GetTypeOfProperty(string propertyName);
        bool TryGetTypeOfProperty(string propertyName, out Type propertyType);

        // Consider removing these and including them in IPropBag
        IPropGen GetPropGen(string propertyName, Type propertyType);
        IProp<T> GetTypedProp<T>(string propertyName);

        TypeSafePropBagMetaData GetMetaData();
    }
}
