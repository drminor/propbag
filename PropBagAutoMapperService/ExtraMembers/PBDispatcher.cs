using DRM.TypeSafePropertyBag;
using System;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    public static class PBDispatcher
    {
        public static object GetValue(ITypeSafePropBag host, string propertyName, Type propertyType)
        {
            return host.GetValWithType(propertyName, propertyType);
        }

        public static void SetValue(ITypeSafePropBag host, string propertyName, Type propertyType, object value)
        {
            host.SetValWithType(propertyName, propertyType, value);
        }
    }
}
