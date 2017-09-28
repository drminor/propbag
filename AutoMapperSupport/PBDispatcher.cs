using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public static class PBDispatcher
    {
        public static object GetValue(object host, string propertyName, Type propertyType)
        {
            return ((IPropBag)host).GetValWithType(propertyName, propertyType);
        }

        public static void SetValue(object host, string propertyName, Type propertyType, object value)
        {
            ((IPropBag)host).SetValWithType(propertyName, propertyType, value);
        }
    }
}
