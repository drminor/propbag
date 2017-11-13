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

        // These could possible be added to an internal interface; 
        // an interface that would be implemented by the PropStore or PropStore Access Service.
        //IPropGen GetPropGen(string propertyName, Type propertyType);
        //IProp<T> GetTypedProp<T>(string propertyName);

        ITypeSafePropBagMetaData GetMetaData();
    }
}
