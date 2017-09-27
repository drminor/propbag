using System;

using DRM.PropBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public static class PBDispatcher
    {
        static object GetValue(object host, string propertyName, Type propertyType)
        {
            return ((IPropBag)host).GetValWithType(propertyName, propertyType);
        }

        static void SetValue(object host, string propertyName, Type propertyType, object value)
        {
            ((IPropBag)host).SetValWithType(propertyName, propertyType, value);
        }
    }
}
