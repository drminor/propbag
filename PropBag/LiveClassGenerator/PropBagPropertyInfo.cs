using System;
using System.Globalization;
using System.Reflection;

using AutoMapper;

namespace DRM.PropBag.LiveClassGenerator
{
    public class PropBagPropertyInfo : PropertyInfo
    {
        Type _hostType;
        Func<object> _getter;
        public Action<object,object> _setter;

        public PropBagPropertyInfo(string name, Type propertyType, Type hostType,
            Func<object> getter,
            Action<object,object> setter)
        {
            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _getter = getter;
            _setter = setter;
        }

        public override Type PropertyType { get; }

        public override PropertyAttributes Attributes => PropertyAttributes.None;

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override string Name { get; }

        public override Type DeclaringType => _hostType;

        public override Type ReflectedType => _hostType;

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            // We have no private accessors, so base.GetAcessors is equivalent regardless of the value of the nonPublic argument.
            return base.GetAccessors();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[] { new ProxyPropertyAttribute("Test") };
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if(attributeType == typeof(ProxyPropertyAttribute))
            {
                return new Attribute[] { new ProxyPropertyAttribute("Test") };
            }
            return new object[0];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return _getter.Method;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return _setter.Method;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            object[] parameters = new object[1] { obj };
            return _getter.Method.Invoke(_getter.Target, invokeAttr, binder, parameters, culture);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            _setter.Method.Invoke(_setter.Target, invokeAttr, binder, new object[] { obj, value }, culture);
        }
    }

}
