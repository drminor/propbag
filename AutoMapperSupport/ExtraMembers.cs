using AutoMapper;
using AutoMapper.ExtraMembers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    class ExtraMembers
    {
    }


    public class When_configuring_extra_members
    {
        public class Source
        {
            //string _item1;
            //public string GetItem1()
            //{
            //    return _item1;
            //}

            public string Item1 { get; set; }
            //(string value)
            //{
            //    _item1 = value;
            //}

            public int MyInteger;
        }

        public class Destination : IHaveExtraMembers
        {
            string _item1;
            int _myInteger;

            public object GetIt(string propertyName, Type propertyType)
            {
                switch (propertyName)
                {
                    case "Item1":
                        {
                            if (propertyType != typeof(string)) throw new InvalidCastException("Item1 is a string.");
                            return _item1;
                        }

                    case "MyInteger":
                        {
                            if (propertyType != typeof(int)) throw new InvalidCastException("MyInteger is an int.");
                            return _myInteger;
                        }
                    default: return null;
                }
            }

            public void SetItWithType(string propertyName, Type propertyType, object value)
            {
                switch (propertyName)
                {
                    case "Item1":
                        {
                            if (value.GetType() != typeof(string)) throw new InvalidCastException("Item1 is a string.");
                            _item1 = (string)value; break;
                        }
                    case "MyInteger":
                        {
                            if (value.GetType() != typeof(int)) throw new InvalidCastException("MyInteger is an int.");
                            _myInteger = (int)value; break;
                        }
                }
            }

            public object GetValueGen(object host, string propertyName, Type propertyType)
            {
                object r = ((IHaveExtraMembers)host).GetIt(propertyName, propertyType);
                return r;
            }

            public void SetValueGen(object host, string propertyName, Type propertyType, object value)
            {
                ((IHaveExtraMembers)host).SetItWithType(propertyName, propertyType, value);
            }
        }




        public void ExtraMembersShouldBeIncluded()
        {
            Dictionary<string, Type> props = new Dictionary<string, Type>
            {
                { "Item1", typeof(string) },
                { "MyInteger", typeof(int) }
            };

            List<MemberInfo> extraMembers = GetExtraMembers<Destination>(props, new Destination(), useStandardDelegates: true);
            //List<MemberInfo> extraMembers = GetExtraMembersStandard<Destination>(props, new Destination(), useStandardDelegates: true);


            //MemberInfo f = extraMembers.First();
            //bool fp = f.IsPublic();

            var config = new MapperConfiguration(cfg =>
            {
                //cfg
                //.AddMemberConfiguration()
                //.AddMember<NameSplitMember>()
                //.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get")
                //        .AddStrings(p => p.DestinationPostfixes, "Set"));

                //cfg.ShouldMapField = ShouldMap;
                //cfg.ShouldMapProperty = ShouldMap;


                //cfg.IncludeExtraMembersForType(typeof(Destination), extraMembers);

                cfg.DefineExtraMemberGetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetGetterStrategy);
                cfg.DefineExtraMemberSetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetSetterStrategy);

                cfg
                .CreateMap<Source, Destination>()
                //.RegisterExtraMembers(cfg)
                .AddExtraDestintionMembers(extraMembers)
                .ForMember("Item1", opt => opt.Condition(srcA => srcA.Item1 != "AA"));

                Func<Destination, bool> cond = (s => "x" != (string)s.GetIt("Item1", typeof(string)));

                cfg
                .CreateMap<Destination, Source>()
                .AddExtraSourceMembers(extraMembers)
                .ForMember("Item1", opt => opt.Condition(cond));

            });


            var mapper = config.CreateMapper();

            Source src = new Source
            {
                Item1 = "This is it",
                MyInteger = 2
            };

            //src.SetItem1("This is it");

            //var newDest = mapper.Map<Source, Destination>(src);
            //newDest.GetIt("Item1", typeof(string)).ShouldBe("This is it");
            //newDest.GetIt("MyInteger", typeof(int)).ShouldBe(2);

            //var newDest2 = mapper.Map<Source, Destination>(src);

            //Destination destinationUsedAsSource = new Destination();
            //destinationUsedAsSource.SetItWithType("Item1", typeof(string), "This is the initial value of src2.Item1");
            //destinationUsedAsSource.SetItWithType("MyInteger", typeof(int), -1);

            //var newSource = mapper.Map<Destination, Source>(destinationUsedAsSource);
            //newSource.Item1.ShouldBe("This is the initial value of src2.Item1");
            //newSource.MyInteger.ShouldBe(-1);
        }

        //public bool ShouldMap(MemberInfo mi)
        //{
        //    if (mi.IsPublic()) return true;

        //    Attribute[] atts = mi.GetCustomAttributes(true) as Attribute[];
        //    if (atts == null) return false;
        //    Attribute test = atts.FirstOrDefault(a => a is ExtraMemberAttribute);

        //    return test != null;
        //}

        /// <summary>
        /// This assumes that mi will always be a PropertyInfo.
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="sourceType"></param> 
        /// <returns></returns>
        public ExtraMemberCallDetails GetGetterStrategy(MemberInfo mi, Expression destination, Type sourceType, IPropertyMap propertyMap)
        {
            Expression indexExp = Expression.Constant(new object[] { sourceType });

            Expression[] parameters = new Expression[3] { Expression.Constant(mi), destination, indexExp };
            return new ExtraMemberCallDetails(ExtraMemberCallDirectionEnum.Get, mi, parameters);
        }

        // This assumes that mi will always be a PropertyInfo.
        public ExtraMemberCallDetails GetSetterStrategy(MemberInfo mi, Expression destination, Type sourceType, IPropertyMap propertyMap, ParameterExpression value)
        {
            Expression newValue;
            if (mi is PropertyInfo pi && pi.PropertyType.IsValueType())
            {
                newValue = Expression.TypeAs((Expression)value, typeof(object));
            }
            else
            {
                newValue = value;
            }

            Expression indexExp = Expression.Constant(new object[] { propertyMap.SourceType });

            Expression[] parameters = new Expression[4] { Expression.Constant(mi), destination, newValue, indexExp };
            return new ExtraMemberCallDetails(ExtraMemberCallDirectionEnum.Set, mi, parameters);
        }

        //private List<DRMWrapperClassGenLib.PropertyDescription> GetPropertyDescriptions(IDictionary<string, Type> typeDefs)
        //{
        //    List<DRMWrapperClassGenLib.PropertyDescription> result = new List<DRMWrapperClassGenLib.PropertyDescription>();

        //    foreach (KeyValuePair<string, Type> kvp in typeDefs)
        //    {
        //        string propertyName = kvp.Key;
        //        Type propertyType = kvp.Value;

        //        result.Add(new DRMWrapperClassGenLib.PropertyDescription(propertyName, propertyType));
        //    }

        //    return result;
        //}

        /// <summary>
        /// The use of the interface IHaveExtraMembers is just one way of producing extra members;
        /// it should not be considered as part of AutoMapper core in any way; 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="props"></param>
        /// <returns></returns>
        private List<MemberInfo> GetExtraMembers<T>(IDictionary<string, Type> props, IHaveExtraMembers bridge, bool useStandardDelegates) where T : IHaveExtraMembers
        {
            List<MemberInfo> result = new List<MemberInfo>();

            foreach (KeyValuePair<string, Type> kvp in props)
            {
                string propertyName = kvp.Key;
                Type propertyType = kvp.Value;

                //BridgeProperty<T> bridge = new BridgeProperty<T>(propertyName, propertyType);

                if (useStandardDelegates)
                {
                    Func<object, string, Type, object> getter =
                        new Func<object, string, Type, object>((host, pn, pt) => ((T)host).GetIt(pn, pt));
                    //    //bridge.GetValue;                        

                    Action<object, string, Type, object> setter =
                       new Action<object, string, Type, object>((host, pn, pt, value) => ((T)host).SetItWithType(pn, pt, value));
                    //    //bridge.SetValue;

                    PropertyInfoWT pi = new PropertyInfoWT(propertyName, propertyType, typeof(T),
                        getter, setter);

                    result.Add(pi);
                }
                else
                {
                    MethodInfo getter = typeof(IHaveExtraMembers).GetMethod("GetValueGen");
                    MethodInfo setter = typeof(IHaveExtraMembers).GetMethod("SetValueGen");

                    PropertyInfoWT pi = new PropertyInfoWT(propertyName, propertyType, typeof(T),
                        bridge, getter, setter);

                    result.Add(pi);
                }

            }

            return result;
        }

        private List<MemberInfo> GetExtraMembersStandard<T>(IDictionary<string, Type> props, IHaveExtraMembers bridge, bool useStandardDelegates) where T : IHaveExtraMembers
        {
            List<MemberInfo> result = new List<MemberInfo>();

            foreach (KeyValuePair<string, Type> kvp in props)
            {
                string propertyName = kvp.Key;
                Type propertyType = kvp.Value;

                if (useStandardDelegates)
                {
                    //Func<object, object> getter =
                    //    new Func<object, object>((host) => ((T)host).GetIt(propertyName, propertyType));

                    //Action<object, object> setter =
                    //    new Action<object, object>((host, value) => ((T)host).SetItWithType(propertyName, propertyType, value));

                    Func<object, string, Type, object> getter =
                        new Func<object, string, Type, object>((host, pn, pt) => ((T)host).GetIt(pn, pt));

                    Action<object, string, Type, object> setter =
                       new Action<object, string, Type, object>((host, pn, pt, value) => ((T)host).SetItWithType(pn, pt, value));

                    PropertyInfoStandard pi = new PropertyInfoStandard(propertyName, propertyType, typeof(T),
                        getter, setter);

                    result.Add(pi);
                }
                else
                {
                    //MethodInfo getter = typeof(IHaveExtraMembers).GetMethod("GetIt", BindingFlags.Public | BindingFlags.Instance);
                    //MethodInfo setter = typeof(IHaveExtraMembers).GetMethod("SetItWithType", BindingFlags.Public | BindingFlags.Instance);

                    MethodInfo getter = typeof(IHaveExtraMembers).GetMethod("GetValueGen");
                    MethodInfo setter = typeof(IHaveExtraMembers).GetMethod("SetValueGen");

                    PropertyInfoStandard pi = new PropertyInfoStandard(propertyName, propertyType, typeof(T),
                        bridge, getter, setter);

                    result.Add(pi);
                }

            }

            return result;
        }
    }

    public interface IHaveExtraMembers
    {
        object GetIt(string propertyName, Type propertyType);
        void SetItWithType(string propertyName, Type propertyType, object value);

        object GetValueGen(object host, string propertyName, Type propertyType);
        void SetValueGen(object host, string propertyName, Type propertyType, object value);
    }

    public interface IUniversal
    {
        string ToString();
    }

    //public delegate object GetValueDel(object host, string name, Type type);
    //public delegate void SetValueDel(object host, string name, Type type, object value);

    //public class BridgeProperty_NotUsed<T> where T: IHaveExtraMembers
    //{
    //    string _propertyName { get; set; }
    //    Type _propertyType { get; set; }

    //    public BridgeProperty_NotUsed(string propertyName, Type propertyType)
    //    {
    //        _propertyName = propertyName;
    //        _propertyType = propertyType;
    //    }

    //    public object GetValue(object host) 
    //    {
    //        return ((T)host).GetIt(_propertyName, _propertyType);
    //    }

    //    public void SetValue(object host, object value)
    //    {
    //        ((T)host).SetItWithType(_propertyName, _propertyType, value);
    //    }
    //}

    /// <remarks>
    /// This is only used for the ExtraMember tests, it should not be considered as part of AutoMapper core in any way; 
    /// the caller is responsible for retrieving or buiding the Property and/or Field Info objects.
    /// </remarks>


    /// <summary>
    /// This implementation requires that the Getter and Setter MethodsInfo provided will be called
    /// on an instance (instead of being static methods).
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
        IHaveExtraMembers _bridge;
        MethodInfo _getter;
        MethodInfo _setter;
        Func<object, string, Type, object> _getterFunc;
        Action<object, string, Type, object> _setterAction;
        bool _useMethodInfos;
        IEnumerable<Attribute> _attributes;

        public PropertyInfoWT(string name, Type propertyType, Type hostType,
            Func<object, string, Type, object> getterFunc,
            Action<object, string, Type, object> setterAction,
            IEnumerable<Attribute> attributes = null)
        {
            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _bridge = null;
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _useMethodInfos = false;
            _attributes = attributes;
            if (attributes == null)
            {
                // TODO: Allow caller to provide list of attributes.
            }
        }

        public PropertyInfoWT(string name, Type propertyType, Type hostType,
            IHaveExtraMembers bridge,
            MethodInfo getter,
            MethodInfo setter,
            IEnumerable<Attribute> attributes = null)
        {
            if (bridge == null)
                if (getter.IsStatic || setter.IsStatic || getter.IsPrivate || setter.IsPrivate) throw new ArgumentException("Both the getter and setter MethodInfo arguments must refer to a public static method.");
                else
                if (getter.IsStatic || setter.IsStatic || getter.IsPrivate || setter.IsPrivate) throw new ArgumentException("Both the getter and setter MethodInfo arguments must refer to a public instance method.");

            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _bridge = bridge;
            _getter = getter;
            _setter = setter;
            _useMethodInfos = true;
            _attributes = attributes;
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
                return _getter;
                //Func<object, object> temp = new Func<object, object>((host) => GetValue(host, null));
                //return temp.Method; // Getter.Method; 
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
                //Action<object, object> temp = new Action<object, object>((host, value) => SetValue(host, value, null));
                //return temp.Method; // Setter.Method;
                return _setter;
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

                object[] parameters = new object[3] { obj, Name, pType };
                return _getter.Invoke(_bridge, invokeAttr, binder, parameters, culture);
            }
            else
            {
                return _getterFunc(obj, Name, pType);
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
                object[] parameters = new object[4] { obj, Name, pType, value };
                _setter.Invoke(_bridge, invokeAttr, binder, parameters, culture);
            }
            else
            {
                _setterAction(obj, Name, pType, value);
            }
        }

    }


    public class PropertyInfoStandard : PropertyInfo
    {
        public const string STRATEGEY_KEY = null; // This will cause AutoMapper to use the default strategies.

        Type _hostType;
        IHaveExtraMembers _bridge;
        MethodInfo _getter;
        MethodInfo _setter;
        Func<object, string, Type, object> _getterFunc;
        Action<object, string, Type, object> _setterAction;
        bool _useMethodInfos;


        public PropertyInfoStandard(string name, Type propertyType, Type hostType,
            Func<object, string, Type, object> getterFunc,
            Action<object, string, Type, object> setterAction)
        {
            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _getterFunc = getterFunc;
            _setterAction = setterAction;
            _useMethodInfos = false;
        }

        public PropertyInfoStandard(string name, Type propertyType, Type hostType,
            IHaveExtraMembers bridge,
            MethodInfo getter,
            MethodInfo setter)
        {
            //if (!getter.IsStatic || !setter.IsStatic) throw new ArgumentException("Both the getter and setter MethodInfo arguments must refer to a static method.");

            Name = name;
            PropertyType = propertyType;
            _hostType = hostType;
            _bridge = bridge;
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
            return new Attribute[] { new ExtraMemberAttribute("Test") };
        }

        public T GetCustomAttribute<T>() where T : System.Attribute
        {
            if (typeof(T) == typeof(ExtraMemberAttribute))
            {
                return new ExtraMemberAttribute("Default, System Provided Attribute") as T;
            }
            return null;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(ExtraMemberAttribute))
            {
                return new Attribute[] { new ExtraMemberAttribute("Default, System Provided Attribute") };
            }
            return new Attribute[0];
        }

        // TODO: Make this a lazy singleton
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (_useMethodInfos)
            {
                return _getter;
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
                return _setter;
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
                return _getter.Invoke(_bridge, invokeAttr, binder, parameters, culture);
            }
            else
            {
                return _getterFunc(obj, Name, PropertyType);
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(ExtraMemberAttribute);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (_useMethodInfos)
            {
                object[] parameters = new object[4] { obj, Name, PropertyType, value };
                _setter.Invoke(_bridge, invokeAttr, binder, parameters, culture);
            }
            else
            {
                _setterAction(obj, Name, PropertyType, value);
            }
        }

    }
}
