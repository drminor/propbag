using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public delegate GetExtVal GetExtValGetter(string assemblyName, string className, string methodName,
            string forPropertyName, string forClassName, string forAssemblyName, Type propertyType, Guid tag);

    public delegate SetExtVal GetExtValSetter(string assemblyName, string className, string methodName,
            string forPropertyName, string forClassName, string forAssemblyName, Type propertyType, Guid tag);

    public interface IExtStoreBinder
    {
        GetExtValGetter ExtValGetter { get; }
        GetExtValSetter ExtValSetter { get; }
    }
}
