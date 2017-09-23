using System;

namespace DRM.WrapperGenLib
{
    public interface IWrapperBase
    {
        Type GetTypeOfProperty(string propertyName);

        object GetVal(string propertyName);

        bool SetVal(string propertyName, object value);
    }
}
