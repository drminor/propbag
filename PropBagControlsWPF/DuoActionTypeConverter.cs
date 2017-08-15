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
            if (context == null)
            {
                Action<string, string> act = (x, y) => Console.WriteLine("");
                return new DoWhenChangedAction(act);
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
                    Action<string, string> act = (x, y) => Console.WriteLine("");
                    return new DoWhenChangedAction(act);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        private object doConvertFrom(string s, object xamlRoot)
        {
            if (s == null) return null;

            try
            {
                string[] parts = s.Split('|');

                if (parts.Length < 3) return null;

                string strPropType = parts[0];
                string strOwnerType = parts[1];
                string methodName = parts[2];


                Type rootType = xamlRoot.GetType();
                if(strOwnerType == rootType.ToString())
                {
                    // The typeConverter's context provides a reference to a running instance of the ownerType.

                    Type propType = Type.GetType(strPropType);
                    GetActionRefDelegate ActionGetter = GetTheGetActionRefDelegate(propType);

                    Delegate d = ActionGetter(xamlRoot, rootType, methodName);
                    return new DoWhenChangedAction(d);
                }
                else
                {
                    // Cannot resolve the owner's type.
                    return null;
                }

            } 
            catch (Exception ee)
            {
                string d = ee.Message;
                throw;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);
        }

        // Overrides the ConvertTo method of TypeConverter.
        // We need to return a string.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(DoWhenChangedAction))
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
