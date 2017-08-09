using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    // This is not used. It is something that is being considered to solve the 
    // problem of how to map external stores to an instance of a IPropBag.


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
