using AutoMapper.ExtraMembers;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// 
    /// TODO: Either update this documentation, or provide support for accepting
    /// MethodInfo(s).
    /// 
    /// 
    /// This implementation allows the Getter and Setter MethodsInfo provided to be called
    /// on an instance that implements ITypeSafePropBag or be static methods, in which case
    /// the obj parameter is ignored.
    /// 
    /// 
    /// The Getter must have a signature of Func&lt;object, Type, object> where the first parameter reference the instance on which the 
    /// method will be called, the second parameter is the type of an object which is intended to receive the value produced 
    /// from the getter. The getter MethodInfo's GetValue methods will return an object of the type specified by the property defintion
    /// and will optionally perform type checks using the Type provided by the caller (in the second parameter.)
    /// 
    /// The Setter must have a signature of Action&lt;object, type, object> where the first parameter references the instance on which the 
    /// method will be called, the second paramter is the type of an object which is intended to be supplied as the value of the set calls,
    /// and the third parameter is the value that will be assigned to the property. The setter will optionally perform tyep checks 
    /// usig the Type provided by the caller (in the second parameter.)
    /// 
    /// If the getter is called without a type parameter, the type of the property will be supplied to the Getter Method.
    /// Likewise for the setter.
    /// 
    /// There are two constructors, one that takes a pair of MethodInfos, and one that takes a pair of delegates with the same signatures as described above.
    /// If MethodInfos are used, an instance of IHaveExtraMembers must be provided on which the MethodInfos will be called.
    /// If the MethodInfos reference static methods, then this "bridge" object should be set to null.
    ///  
    /// No such target is needed for the pair of delegates, since they incorporate the target in their definition.
    /// If anonmous delegates are used, these will be private, static methods. AutoMapper defaults to only considering
    /// source members that are public, instance methods therefor in order for this to work you must supply a custom 
    /// ShouldMapProperty and ShouldMapField functions.
    /// </summary>
    public class PropertyInfoWT : PropertyInfo
    {
        public const string STRATEGEY_KEY = "WithType";

        Type _hostType;
        //IHaveExtraMembers _bridge;
        //MethodInfo _getter;
        //MethodInfo _setter;
        Func<ITypeSafePropBag, string, Type, object> _getterFunc;
        Action<ITypeSafePropBag, string, Type, object> _setterAction;
        bool _useMethodInfos;
        IEnumerable<Attribute> _attributes;

        public PropertyInfoWT(string name, Type propertyType, Type hostType,
            Func<ITypeSafePropBag, string, Type, object> getterFunc,
            Action<ITypeSafePropBag, string, Type, object> setterAction,
            IEnumerable<Attribute> attributes = null)
        {
            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            //_bridge = null;
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _useMethodInfos = false;
            _attributes = attributes;
            if (attributes == null)
            {
                // TODO: Allow caller to provide list of attributes.
            }
        }

        //public PropertyInfoWT(string name, Type propertyType, Type hostType,
        //    IHaveExtraMembers bridge,
        //    MethodInfo getter,
        //    MethodInfo setter,
        //    IEnumerable<Attribute> attributes = null)
        //{
        //    if (bridge == null)
        //        if (getter.IsStatic || setter.IsStatic || getter.IsPrivate || setter.IsPrivate) throw new ArgumentException("Both the getter and setter MethodInfo arguments must refer to a public static method.");
        //        else
        //        if (getter.IsStatic || setter.IsStatic || getter.IsPrivate || setter.IsPrivate) throw new ArgumentException("Both the getter and setter MethodInfo arguments must refer to a public instance method.");

        //    Name = name;
        //    PropertyType = propertyType;
        //    _hostType = hostType;
        //    _bridge = bridge;
        //    _getter = getter;
        //    _setter = setter;
        //    _useMethodInfos = true;
        //    _attributes = attributes;
        //}

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
            return new Attribute[] { new ExtraMemberAttribute("Test", STRATEGEY_KEY) };
        }

        public T GetCustomAttribute<T>() where T : System.Attribute
        {
            if (typeof(T) == typeof(ExtraMemberAttribute))
            {
                return new ExtraMemberAttribute("Default, System Provided Attribute", STRATEGEY_KEY) as T;
            }
            return null;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(ExtraMemberAttribute))
            {
                return new Attribute[] { new ExtraMemberAttribute("Default, System Provided Attribute", STRATEGEY_KEY) };
            }
            return new Attribute[0];
        }

        // TODO: Make this a lazy singleton
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (_useMethodInfos)
            {
                return null;
                //return _getter;
                ////Func<object, object> temp = new Func<object, object>((host) => GetValue(host, null));
                ////return temp.Method; // Getter.Method; 
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
                return null;
                ////Action<object, object> temp = new Action<object, object>((host, value) => SetValue(host, value, null));
                ////return temp.Method; // Setter.Method;
                //return _setter;
            }
            else
            {
                return _setterAction.Method;
            }

        }
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            Type pType;
            if (index != null && index.Length > 0)
            {
                pType = (Type)index[0];
            }
            else
            {
                pType = PropertyType;
            }

            if (_useMethodInfos)
            {
                //object[] parameters = new object[3] { obj, Name, pType };
                //return _getter.Invoke(_bridge, invokeAttr, binder, parameters, culture);
                return null;
            }
            else
            {
                return _getterFunc( (ITypeSafePropBag)obj, Name, pType);
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(ExtraMemberAttribute);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            Type pType;
            if (index != null && index.Length > 0)
            {
                pType = (Type)index[0];
            }
            else
            {
                pType = PropertyType;
            }

            if (_useMethodInfos)
            {
                //object[] parameters = new object[4] { obj, Name, pType, value };
                //_setter.Invoke(_bridge, invokeAttr, binder, parameters, culture);
            }
            else
            {
                _setterAction((ITypeSafePropBag)obj, Name, pType, value);
            }
        }

    }
}
