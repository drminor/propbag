using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.ComponentModel.Design.Serialization;

using DRM.PropBag.ControlModel;
using System.Xaml;

namespace DRM.PropBag.ControlsWPF
{
    public class DuoActionTypeConverter : TypeConverter
    {
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return base.CanConvertFrom(context, sourceType);
            }
        }

        // Overrides the ConvertFrom method of TypeConverter.
        // We need to return a DoWhenChangedAction
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // If the context is null, or if the context doesn't provide a "live" root object,
            // this returns a "placeholder" value. 
            // We are assuming that these calls are being made before the "entire XAML environment" has been setup,
            // and that eventually we will be called once the environment has been setup.

            if (context == null)
            {
                return PlaceholderDelegate;
            }

            IRootObjectProvider rootProvider = context.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (rootProvider != null && value is String)
            {
                if(rootProvider.RootObject != null)
                {
                    object result = doConvertFrom((string)value, rootProvider.RootObject);
                    if (result != null) return result;
                }
                else
                {
                    return PlaceholderDelegate;
                }
            }
            else
            {
                return PlaceholderDelegate;
            }

            return base.ConvertFrom(context, culture, value);
        }

        private object doConvertFrom(string s, object xamlRoot)
        {
            // TODO: create a custom exeception type for this.
            if (s == null) throw new ApplicationException("The XamlRoot is null");

            try
            {
                string[] parts = s.Split('|');

                if (parts.Length != 3)
                    throw new ApplicationException("Value does not have exactly three parts, separated by |.");

                string strPropType = parts[0];
                string strTargetType = parts[1]; // The name of the class that declares or "hosts" the method.
                string methodName = parts[2];

                // The typeConverter's context is a class that should have a property that provides access to an object 
                // of the class that declares the method we are looking for.
                // This property will be marked with the PropBagInstanceAttribute and will have
                // the TargetType's class name as its PropBagTemplate value.
                object targetInstance = ReflectionHelpers.GetTargetInstance(xamlRoot, strTargetType);
                if(targetInstance == null)
                    throw new ApplicationException(string.Format("Cannot find a reference to a running instance of {0}, and cannot find a public constructor to use to create a running instance.", strTargetType));

                Type targetType = targetInstance.GetType();
                if (strTargetType != targetType.ToString()) 
                    throw new ApplicationException(string.Format("The {0} doesn't match the {1}", strTargetType, targetType.ToString()));

                Type propType = Type.GetType(strPropType);
                GetActionRefDelegate ActionGetter = GetTheGetActionRefDelegate(propType);

                Delegate d = ActionGetter(targetInstance, targetType, methodName);
                return new DoWhenChangedAction(d);

            } 
            catch (Exception ee)
            {
                // This for testing.
                string d = ee.Message;
                throw;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        // Overrides the ConvertTo method of TypeConverter.
        // We need to return a string.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value is Delegate)
                {
                    MethodInfo mi = ((Delegate)value).GetMethodInfo();
                    string pt = mi.GetParameters()[0].ParameterType.FullName.ToString();
                    return string.Format("{0}|{1}|{2}", pt, mi.DeclaringType, mi.Name);
                }
            }
            else
            {
                //int a = 0;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        //public override bool IsValid(ITypeDescriptorContext context, object value)
        //{
        //    return true;
        //}

        // TODO: Should this return a value with the correct parameter types?
        private DoWhenChangedAction PlaceholderDelegate 
        {
            get
            {
                Action<string, string> act = (x, y) => new object();
                return new DoWhenChangedAction(act);
            }
        }

        #region Helper Methods for the Generic Method Templates

        // Delegate declarations.
        private delegate Delegate GetActionRefDelegate(object owningInstance, Type ownerType, string methodName);

        private static GetActionRefDelegate GetTheGetActionRefDelegate(Type propertyType)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetActionRefDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            GetActionRefDelegate result = (GetActionRefDelegate)Delegate.CreateDelegate(typeof(GetActionRefDelegate), methInfoGetProp);

            return result;
        }

        #endregion
    }

    #region Generic Method Templates

    static class GenericMethodTemplates
    {
        private static Delegate GetActionRefDelegate<T>(object owningInstance, Type ownerType, string methodName)
        {
            MethodInfo mi = ownerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (!IsDuoAction<T>(mi)) return null;

            Action<T, T> del = (Action<T, T>)Delegate.CreateDelegate(typeof(Action<T, T>), owningInstance, mi);

            return del;
        }


        static private bool IsDuoAction<T>(MethodInfo mi)
        {
            if (mi.ReturnType != typeof(void))
            {
                // Must return void.
                return false;
            }

            ParameterInfo[] parms = mi.GetParameters();

            if ((parms.Length != 2) || (parms[0].ParameterType != typeof(T)) || (parms[1].ParameterType != typeof(T)))
            {
                // Must have two parameters of the specified type.
                return false;
            }
            return true;
        }

    }

    #endregion
}
