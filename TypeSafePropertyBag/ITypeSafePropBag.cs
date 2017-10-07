using System;

namespace DRM.TypeSafePropertyBag
{
    public interface ITypeSafePropBag
    {
        Type GetTypeOfProperty(string propertyName);

        bool TryGetTypeOfProperty(string propertyName, out Type type);

        object GetValWithType(string propertyName, Type propertyType);

        bool SetValWithType(string propertyName, Type propertyType, object value);

        T GetIt<T>(string propertyName);

        // TODO: Consider adding GetTypedProp
        //IProp<T> GetTypedProp<T>(string propertyName);

        bool SetIt<T>(T value, string propertyName);
    }
}
