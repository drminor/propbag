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
using System.Windows.Markup;

namespace DRM.PropBag.ControlsWPF
{
    public class ComparerFuncTypeConverter : TypeConverter
    {
        static private Type GMT_TYPE = typeof(GenericMethodTemplatesComp);


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
                return Placeholder;
            }

            IRootObjectProvider rootProvider = context.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (rootProvider != null && value is String)
            {
                if (rootProvider.RootObject != null)
                {

                    object result = doConvertFrom((string)value);
                    if (result != null) return result;
                    return Placeholder;
                }
                else
                {
                    return Placeholder;
                }
            }
            else
            {
                return Placeholder;
            }

            //return base.ConvertFrom(context, culture, value);
        }

        private object doConvertFrom(string s)
        {
            // TODO: create a custom exeception type for this.
            if (s == null) throw new ApplicationException("The string to convert is null.");

            // TODO: Consider using the IProviderValueTarget to get the property type.
            //IProvideValueTarget valTargetProvider = context.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            //if (valTargetProvider != null)
            //{
            //    System.Windows.Controls.Control cc = valTargetProvider.TargetObject as System.Windows.Controls.Control;
            //    System.Windows.Controls.Control pp = cc.Parent as System.Windows.Controls.Control;

            //    if (pp is PropItem)
            //    {
            //        Type pt = ((PropItem)pp).PropertyType;
            //    }
            //}
            

            try
            {
                string[] parts = s.Split('|');

                if (parts.Length < 3)
                    throw new ApplicationException("Value does not have three or more parts, separated by |.");

                string strPropType = parts[0];
                int formatType = int.Parse(parts[1]);

                // TODO: Support the following additional format types:
                // 1 = Type|1|ClassName|MethodName -- where the class is a generic class that can be instantiated,
                //          and has a method, named MethodName that 
                //          that compares two objects of the prop type and returns bool.
                // 2 = Type|2|ClassName|PropertyName|MethodName -- Same as #1, except that
                //          the Property named PropertyName, returns the generic class
                // 3 - Type|3|ClassName|PropertyName|MethodName -- Same as #2, except that
                //          the class is not generic -- it returns the comparer of the proper type
                //          without the type needing to be specified.


                if (formatType == 0)
                {
                    string targetType = parts[2];
                    object instance;
                    MethodInfo mi = GetMIFromClassWithDefaultProp(strPropType, targetType, out instance);

                    if (mi == null)
                        throw new ApplicationException("Cannot find a class with that name that has a Default property that returns an object that implements the IEqualityComparer<T> interface.");

                    Type propType = Type.GetType(strPropType);
                    GetComparerFuncDelegate FuncGetter = GetTheGetComparerFuncDelegate(propType);

                    Delegate d = FuncGetter(mi, instance);
                   
                    return new PropComparerFunc(d);

                }
                else
                {
                    throw new ApplicationException("Only format type 0 is supported.");
                }
            }
            catch (Exception ee)
            {
                // This for testing.
                string d = ee.Message;
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strTargetType">The name of the class that declares or "hosts"
        /// a static property named default, which returns a class that has an Equals method.</param>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        private MethodInfo GetMIFromClassWithDefaultProp(string strPropType, string strTargetType,
            out object instance)
        {
            instance = null;

            string tn = GetTypeName(strPropType, strTargetType);
            Type declaringType = Type.GetType(GetTypeName(strPropType, strTargetType));

            if (declaringType == null) return null;

            //PropertyInfo[] list = declaringType.GetProperties();

            //PropertyInfo[] list2 = declaringType.GetProperties(BindingFlags.Static);

            PropertyInfo pi = declaringType.GetProperty("Default");

            // Get the value of the Default property -- An instance of EqualityComparer<strPropType>
            instance =  pi.GetValue(null);

            // Get a MethodInfo object from the Eqauls Method that takes two parameters.
            Type cType = instance.GetType();
            Type propType = Type.GetType(strPropType);
            return cType.GetMethod("Equals", new Type[] {propType, propType});
        }

        // TODO: Consider writing a method that takes an array of property Types.
        private string GetTypeName(string strPropType, string strTargetType)
        {
            return string.Format("{0}`1[{1}]", strTargetType, strPropType);
            //Type.GetType("System.Collections.Generic.IEnumerable`1[System.String]");
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
        private PropComparerFunc Placeholder
        {
            get
            {
                Func<string, string, bool> compr = RefEqualityComparer<string>.Default.Equals;
                return new PropComparerFunc(compr);
            }
        }

        #region Helper Methods for the Generic Method Templates

        // Delegate declarations.
        private delegate Delegate GetComparerFuncDelegate(MethodInfo mi, object owningInstance);

        private static GetComparerFuncDelegate GetTheGetComparerFuncDelegate(Type propertyType)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetComparerFuncDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
            GetComparerFuncDelegate result = (GetComparerFuncDelegate)Delegate.CreateDelegate(typeof(GetComparerFuncDelegate), methInfoGetProp);

            return result;
        }

        #endregion
    }

    #region Generic Method Templates

    static class GenericMethodTemplatesComp
    {
        private static Delegate GetComparerFuncDelegate<T>(MethodInfo mi, object owningInstance)
        {
            if (!IsComparerFunc<T>(mi)) return null;

            Func<T, T, bool> del = (Func<T, T, bool>)Delegate.CreateDelegate(typeof(Func<T, T, bool>), owningInstance, mi);

            return del;

        }

        static private bool IsComparerFunc<T>(MethodInfo mi)
        {
            if (mi.ReturnType != typeof(bool))
            {
                // Must return bool.
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

