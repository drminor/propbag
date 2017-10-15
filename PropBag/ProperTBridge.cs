using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    // Yet another way to provide Type-safe access when the Type is not know at
    // runtime. 
    public class ProperTBridge<T> : IProperTBridge
    {
        public ProperTBridge() { }

        public object GetValue(IPropBagMin host, string propertyName)
        {
            IProp<T> propT = host.GetTypedProp<T>(propertyName);

            return propT.TypedValue;
        }

        public void SetValue(IPropBagMin host, string propertyName, object value)
        {
            IProp<T> propT = host.GetTypedProp<T>(propertyName);

            propT.TypedValue = (T) value;
        }
    }

    public class ProperTBridgeCreator
    {
        public static ProperTBridgeActivator GetActivator(Type type)
        {
            if (type == null)
            {
                throw new NullReferenceException("type");
            }

            ConstructorInfo emptyConstructor = type.GetConstructor(Type.EmptyTypes);

            DynamicMethod dynamicMethod = new DynamicMethod("CreateInstance", type, Type.EmptyTypes, true);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
            ilGenerator.Emit(OpCodes.Ret);

            return (ProperTBridgeActivator)dynamicMethod.CreateDelegate(typeof(ProperTBridgeActivator));
        }
    }

    public delegate IProperTBridge ProperTBridgeActivator();
}
