using System;

using System.Reflection;

namespace DRM.PropBagControlsWPF
{
    public class ReflectionHelpers
    {
        public const string DEFAULT_INSTANCE_KEY = "main";

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
                return System.Activator.CreateInstance(pt, null);
            }
            else
            {
                if (HasSpecialConstructor(pt))
                {
                    // Ok, lets create a new one using the special constructor which takes a byte value of 0xFF.
                    byte flags = PropBagTemplate.TEST_FLAG;
                    result = System.Activator.CreateInstance(pt, new object[] { flags });
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

        #region NOT USED

        //static public void CreateTargetAndAssign(object propertyHost, PropertyInfo propBagClassProperty,
        //    Type propModelType, object propModel)
        //{
        //    Type targetType = propBagClassProperty.PropertyType;
        //    ConstructorInfo ci = GetPropModelConstructor(targetType, propModelType);
        //    object newInstance = ci.Invoke(new object[] { propModel });

        //    propBagClassProperty.SetValue(propertyHost, newInstance);
        //}

        #endregion

    }
}
