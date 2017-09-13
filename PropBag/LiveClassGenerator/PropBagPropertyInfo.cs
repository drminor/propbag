using System;
using System.Globalization;
using System.Reflection;

namespace DRM.PropBag.LiveClassGenerator
{
    public class ProxyPropertyInfo : PropertyInfo
    {
        Type _hostType;
        MethodInfo _getter;
        MethodInfo _setter;
        Func<object, object> _getterFunc;
        Action<object, object> _setterAction;
        bool _useMethodInfos;


        public ProxyPropertyInfo(string name, Type propertyType, Type hostType,
            Func<object, object> getterFunc,
            Action<object, object> setterAction)
        {
            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _useMethodInfos = false;
        }

        public ProxyPropertyInfo(string name, Type propertyType, Type hostType,
            MethodInfo getter,
            MethodInfo setter)
        {
            if (!getter.IsStatic || !setter.IsStatic) throw new ArgumentException("Both the getter and setter MethodInfo arguments must refer to a static method.");

            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _getter = getter;
            _setter = setter;
            _useMethodInfos = true;
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
            return new Attribute[] { new AutoMapper.ExtraPropertyAttribute("Test") };
        }

        public T GetCustomAttribute<T>() where T : System.Attribute
        {
            if (typeof(T) == typeof(AutoMapper.ExtraPropertyAttribute))
            {
                return new AutoMapper.ExtraPropertyAttribute("Default, System Provided Attribute") as T;
            }
            return null;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(AutoMapper.ExtraPropertyAttribute))
            {
                return new Attribute[] { new AutoMapper.ExtraPropertyAttribute("Default, System Provided Attribute") };
            }
            return new Attribute[0];
        }

        // TODO: Make this a lazy singleton
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (_useMethodInfos)
            {
                Func<object, object> temp = new Func<object, object>((host) => GetValue(host, null));
                return temp.Method; // Getter.Method; 
            }
            else
            {
                return _getterFunc.Method;
            }
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        // TODO: Make this a lazy singleton
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (_useMethodInfos)
            {
                Action<object, object> temp = new Action<object, object>((host, value) => SetValue(host, value, null));
                return temp.Method; // Setter.Method;
            }
            else
            {
                return _setterAction.Method;
            }

        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (_useMethodInfos)
            {
                object[] parameters = new object[3] { obj, Name, PropertyType };
                return _getter.Invoke(null, invokeAttr, binder, parameters, culture);
            }
            else
            {
                return _getterFunc(obj);
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(AutoMapper.ExtraPropertyAttribute);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (_useMethodInfos)
            {
                object[] parameters = new object[4] { obj, Name, PropertyType, value };
                _setter.Invoke(null, invokeAttr, binder, parameters, culture);
            }
            else
            {
                _setterAction(obj, value);
            }
        }

    }
}
