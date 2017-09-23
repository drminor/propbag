using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace DRM.PropBag.ControlsWPF
{

    public class ReflectionHelpers
    {
        public const string DEFAULT_INSTANCE_KEY = "main";

        static public PropertyInfo GetPropBagClassProperty(Type declaringType, string className, string instanceKey)
        {
            PropertyInfo[] propDefs = declaringType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            for (int pPtr = 0; pPtr < propDefs.Length; pPtr++)
            {
                if (IsPropBagInstanceWithCorrectType(propDefs[pPtr], className, instanceKey))
                {
                    return propDefs[pPtr];
                }
            }
            return null;
        }

        static public void CreateTargetAndAssign(object propertyHost, PropertyInfo propBagClassProperty,
            Type propModelType, object propModel)
        {
            Type targetType = propBagClassProperty.PropertyType;
            ConstructorInfo ci = GetPropModelConstructor(targetType, propModelType);
            object newInstance = ci.Invoke(new object[] { propModel });

            propBagClassProperty.SetValue(propertyHost, newInstance);

        }

        /// <summary>
        /// Returns an instance of the class that is returned by the property marked with the PropBagInstanceAttribute attribute.
        /// </summary>
        /// <param name="xamlRoot"></param>
        /// <param name="strTargetType"></param>
        /// <returns></returns>
        static public object GetTargetInstance(object xamlRoot, string strTargetType, string instanceKey)
        {
            Type rootType = xamlRoot.GetType();

            PropertyInfo classProperty = GetPropBagClassProperty(rootType, strTargetType, instanceKey);

            if (classProperty != null)
            {
                return GetRunningInstance(classProperty, xamlRoot);
            }

            return null;
        }

        static public bool IsPropBagInstanceWithCorrectType(PropertyInfo propDef, string strTargetType, string instanceKey)
        {
            //IEnumerable<System.Attribute> list = propDef.GetCustomAttributes();
            System.Attribute att = propDef.GetCustomAttribute(typeof(PropBagInstanceAttribute));
            if (att == null) return false;

            //return DoNameSpacesMatch(propDef.PropertyType.ToString(), strTargetType);

            PropBagInstanceAttribute pbia = (PropBagInstanceAttribute)att;
            return (pbia.InstanceKey == instanceKey);
            //return DoNameSpacesMatch(pbia.PropBagTemplate, strTargetType);
        }

        static public bool DoNameSpacesMatch(string ns1, string ns2)
        {
            int cnt1 = ns1.Count(x => x == '.');
            int cnt2 = ns2.Count(x => x == '.');

            if (cnt1 == cnt2)
            {
                return string.Equals(ns1, ns2, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                if (cnt1 < cnt2)
                {
                    //var x = ns2.Split('.').Skip(cnt2 - cnt1);
                    //var y = string.Join<string>(".", x);

                    string localizedNs2 = string.Join(".", ns2.Split('.').Skip(cnt2 - cnt1));
                    return string.Equals(ns1, localizedNs2, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    string localizedNs1 = string.Join(".", ns1.Split('.').Skip(cnt1 - cnt2));
                    return string.Equals(localizedNs1, ns2, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        static public object GetRunningInstance(PropertyInfo propDef, object propsParent)
        {
            // Let's see if the property has been initialized, and if so we will use it's value.
            MethodInfo mi = propDef.GetMethod;
            object result = mi.Invoke(propsParent, null);
            if (result != null) return result;

            // Ok, lets try creating one using the default, public constructor which takes no parameters.
            Type pt = propDef.PropertyType;
            if (HasDefaultPublicConstructor(pt))
            {
                return Activator.CreateInstance(pt, null);
            }
            else
            {
                if (HasSpecialConstructor(pt))
                {
                    // Ok, lets create a new one using the special constructor which takes a byte value of 0xFF.
                    byte flags = PropBagTemplate.TEST_FLAG;
                    result = Activator.CreateInstance(pt, new object[] { flags });
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        static public bool HasDefaultPublicConstructor(Type targetType)
        {
            return null != targetType.GetConstructor(Type.EmptyTypes);
        }

        static public bool HasSpecialConstructor(Type targetType)
        {
            Type[] types = new Type[1];
            types[0] = typeof(byte);
            return null != targetType.GetConstructor(types);
        }

        /// <summary>
        /// Build a reference to constructor of the type that will inherit
        /// from this IPropBag that takes a single (propModel) argument.
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="propModelType"></param>
        /// <returns></returns>
        static public ConstructorInfo GetPropModelConstructor(Type targetType, Type propModelType)
        {
            Type[] types = new Type[] { propModelType };
            //types[0] = propModelType;
            return targetType.GetConstructor(types);
        }

    }
}
