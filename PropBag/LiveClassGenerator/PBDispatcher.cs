using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.LiveClassGenerator
{
    public static class PBDispatcher
    {
        static object GetValue(object host, string propertyName, Type propertyType)
        {
            return ((IPropBag)host).GetIt(propertyName, propertyType);
        }

        static void SetValue(object host, string propertyName, Type propertyType, object value)
        {
            ((IPropBag)host).SetItWithType(value, propertyType, propertyName);
        }
    }
}
