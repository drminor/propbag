using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using DRM.TypeSafePropertyBag;
using System.Collections.ObjectModel;

namespace DRM.PropBag
{
    public abstract class AbstractPropFactory : IPropFactory
    {
        #region Public Properties

        public bool ReturnDefaultForUndefined { get; }
        public ResolveTypeDelegate TypeResolver { get; }

        public IConvertValues ValueConverter { get; }

        public abstract bool ProvidesStorage { get; }

        /// <summary>
        /// This is a const string property of System.Windows.Data.Binding,
        /// we are making it an overridable property in case other frameworks use something else.
        /// </summary>
        public virtual string IndexerName { get; }

        #endregion

        #region Constructor

        public AbstractPropFactory(bool returnDefaultForUndefined,
            ResolveTypeDelegate typeResolver = null,
            IConvertValues valueConverter = null)
        {
            ReturnDefaultForUndefined = returnDefaultForUndefined;

            // Use our default implementation, if the caller did not supply one.
            TypeResolver = typeResolver ?? this.GetTypeFromName;

            // Use our default implementation, if the caller did not supply one.
            ValueConverter = valueConverter ?? new PropFactoryValueConverter(null);

            IndexerName = "Item[]";
        }

        #endregion

        #region Collection-type property creators

        public abstract ICPropPrivate<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<CT, CT> doWhenChanged = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        public abstract ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<CT, CT> doWhenChanged = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        #endregion

        #region Propety-type property creators

        public abstract IProp<T> Create<T>(
            T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null);

        public abstract IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T, T, bool> comparer = null);

        #endregion

        #region Generic property creators

        public abstract IPropGen CreateGenFromObject(Type typeOfThisProperty,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null);

        public abstract IPropGen CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null);

        public abstract IPropGen CreateGenWithNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null);

        //public virtual IPropGen CreatePropInferType(object value, string propertyName, object extraInfo, bool hasStorage)
        //{
        //    System.Type typeOfThisValue;
        //    bool typeIsSolid;

        //    if (value == null)
        //    {
        //        typeOfThisValue = typeof(object);
        //        typeIsSolid = false;
        //    }
        //    else
        //    {
        //        typeOfThisValue = GetTypeFromValue(value);
        //        typeIsSolid = true;
        //    }

        //    IPropGen prop = this.CreateGenFromObject(typeOfThisValue, value, propertyName, extraInfo, 
        //        hasStorage, typeIsSolid, null, false, null, false);
        //    return prop;
        //}

        #endregion

        #region Default Value and Type Support

        /// <summary>
        /// Implementors of deriving classes must ensure that the value returned by this 
        /// method will be the same throughout the lifetime of the factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual Func<T, T, bool> GetRefEqualityComparer<T>()
        {
            var y = RefEqualityComparer<T>.Default;

            DRM.PropBag.RefEqualityComparer<T> x = RefEqualityComparer<T>.Default;

            //Func<T, T, bool> result = x.Equals;
            return x.Equals; // result;
            //return RefEqualityComparer<T>.Default;
        }

        public T GetDefaultValue<T>(string propertyName = null)
        {
            return ValueConverter.GetDefaultValue<T>(propertyName);
            //return default(T);
        }

        public object GetDefaultValue(Type propertyType, string propertyName = null)
        {
            return ValueConverter.GetDefaultValue(propertyType, propertyName);
            //if (propertyType == null)
            //{
            //    throw new InvalidOperationException($"Cannot manufacture a default value if the type is specified as null for property: {propertyName}.");
            //}

            //if (propertyType == typeof(string))
            //    return null;

            //return Activator.CreateInstance(propertyType);
        }

        public Type GetTypeFromValue(object value)
        {
            return value?.GetType();
        }

        public virtual bool IsTypeSolid(object value, Type propertyType)
        {
            if (propertyType == null)
            {
                return (value != null && GetTypeFromValue(value) != typeof(object));
            }
            else
            {
                return true;
            }
        }

        public virtual T GetValueFromObject<T>(object value)
        {
            Type t = typeof(T);
            if (value == null)
            {
                if (t.IsValueType) throw new InvalidCastException("Cannot set an object that have a ValueType to null.");
                return (T)(object)null;
            }

            // value is already of the correct type.
            if (t == typeof(object)) return (T)(object)value;

            // TODO: Consider calling  Type s = PropFactory.GetTypeFromValue(value);
            Type vType = value.GetType();

            // value is already of the correct type.
            if (vType == t) return (T)(object)value;

            object parameter = new ControlModel.TwoTypes(t, vType);

            return (T)ValueConverter.ConvertBack(value, t, parameter, CultureInfo.CurrentCulture);
        }

        public virtual CT GetValueFromObject<CT,T>(object value) where CT: class, IEnumerable<T>
        {
            if (value == null)
            {
                return default(CT);
            }

            Type collectionType = typeof(CT);
            //Type t = typeof(T);

            // TODO: Consider calling  Type s = PropFactory.GetTypeFromValue(value);
            Type vType = value.GetType();

            // value is already of the correct type.
            if (vType == collectionType) return (CT)(object)value;

            //object parameter = new ControlModel.TwoTypes(t, s);

            //return (T)ValueConverter.ConvertBack(value, t, parameter, CultureInfo.CurrentCulture);

            throw new InvalidCastException($"Cannot convert object: {value} with type: {vType.ToString()} to {collectionType.Name}.");
        }

        public virtual T GetValueFromString<T>(string value)
        {
            Type t = typeof(T);
            Type s = typeof(string);

            if (t == s)
                return (T)(object)value;

            object parameter = new ControlModel.TwoTypes(t, s);

            return (T)ValueConverter.ConvertBack(value, t, parameter, CultureInfo.CurrentCulture);
        }

        public virtual CT GetValueFromString<CT, T>(string value) where CT : class
        {
            Type collectionType = typeof(CT);

            if (value == null)
            {
                return null;
            }
            else if (value == string.Empty && collectionType == typeof(ObservableCollection<T>))
            {
                return new ObservableCollection<T>() as CT;
            }
            else
            {
                throw new InvalidCastException($"Cannot convert string: {value} to {collectionType.Name}.");
            }
        }

        private int ObservableCollection<T>()
        {
            throw new NotImplementedException();
        }

        public virtual Type GetTypeFromName(string typeName)
        {
            Type result;
            try
            {
                result = Type.GetType(typeName);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", e);
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }

            return result;
        }

        #endregion

        #region Shared Delegate Creation Logic

        //TODO: Consider caching the delegates that are created here.

        static private Type gmtType = typeof(APFGenericMethodTemplates);

        #region Collection-Type Methods

        // From Object
        protected virtual CreateCPropFromObjectDelegate GetCPropCreator(Type collectionType, Type itemType)
        {
            MethodInfo mi = gmtType.GetMethod("CreateCPropFromObject",
                BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);

            CreateCPropFromObjectDelegate result = (CreateCPropFromObjectDelegate)Delegate.
                CreateDelegate(typeof(CreateCPropFromObjectDelegate), mi);

            return result;
        }

        // From String
        protected virtual CreateCPropFromStringDelegate GetCPropFromStringCreator(Type collectionType, Type itemType)
        {
            MethodInfo mi = gmtType.GetMethod("CreateCPropFromString",
                BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);

            CreateCPropFromStringDelegate result = (CreateCPropFromStringDelegate)Delegate.
                CreateDelegate(typeof(CreateCPropFromStringDelegate), mi);

            return result;
        }

        // With No Value
        protected virtual CreateCPropWithNoValueDelegate GetCPropWithNoValueCreator(Type collectionType, Type itemType)
        {
            MethodInfo mi = gmtType.GetMethod("CreateCPropWithNoValue",
                BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);

            CreateCPropWithNoValueDelegate result = (CreateCPropWithNoValueDelegate)Delegate.
                CreateDelegate(typeof(CreateCPropWithNoValueDelegate), mi);

            return result;
        }

        #endregion

        #region Property-Type Methods

        // From Object
        protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropFromObjectDelegate result = (CreatePropFromObjectDelegate)Delegate.CreateDelegate(typeof(CreatePropFromObjectDelegate), mi);

            return result;
        }

        // From String
        protected virtual CreatePropFromStringDelegate GetPropFromStringCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropFromStringDelegate result = (CreatePropFromStringDelegate)Delegate.CreateDelegate(typeof(CreatePropFromStringDelegate), mi);

            return result;
        }

        // With No Value
        protected virtual CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropWithNoValueDelegate result = (CreatePropWithNoValueDelegate)Delegate.CreateDelegate(typeof(CreatePropWithNoValueDelegate), mi);

            return result;
        }

        #endregion Property-Type Methods

        #endregion Shared Delegate Creation Logic
    }

    static class APFGenericMethodTemplates
    {
        #region Collection-Type Methods

        // From Object
        private static ICPropPrivate<CT, T> CreateCPropFromObject<CT,T>(AbstractPropFactory propFactory,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false) where CT : class, IEnumerable<T>
        {
            CT initialValue = propFactory.GetValueFromObject<CT>(value);

            return propFactory.Create<CT,T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<CT, CT>)doWhenChanged, doAfterNotify, GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        }

        // From String
        private static ICPropPrivate<CT, T> CreateCPropFromString<CT, T>(IPropFactory propFactory,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = true) where CT : class,IEnumerable<T>
        {
            CT initialValue;
            if (useDefault)
            {
                initialValue = propFactory.ValueConverter.GetDefaultValue<CT,T>(propertyName);
            }
            else
            {
                initialValue = propFactory.GetValueFromString<CT,T>(value);
            }

            return propFactory.Create<CT, T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<CT, CT>)doWhenChanged, doAfterNotify, GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        }

        // With No Value
        private static ICPropPrivate<CT, T> CreateCPropWithNoValue<CT, T>(IPropFactory propFactory,
            bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = true) where CT : class, IEnumerable<T>
        {
            return propFactory.CreateWithNoValue<CT, T>(propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<CT, CT>)doWhenChanged, doAfterNotify, GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
        }

        private static Func<CT, CT, bool> GetComparerForCollections<CT>(Delegate comparer, IPropFactory propFactory, bool useRefEquality)
        {
            Func<CT, CT, bool> compr;
            if (useRefEquality || comparer == null)
                return propFactory.GetRefEqualityComparer<CT>();
            else
                return compr = (Func<CT, CT, bool>)comparer;
        }

        #endregion

        #region Property-Type Methods

        // From Object
        private static IProp<T> CreatePropFromObject<T>(AbstractPropFactory propFactory,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            T initialValue = propFactory.GetValueFromObject<T>(value);

            return propFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T, T>)doWhenChanged, doAfterNotify, GetComparerForCollections<T>(comparer, propFactory, useRefEquality));
        }

        // From String
        private static IProp<T> CreatePropFromString<T>(AbstractPropFactory propFactory,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            T initialValue;
            if (useDefault)
            {
                initialValue = propFactory.ValueConverter.GetDefaultValue<T>(propertyName);
            }
            else
            {
                initialValue = propFactory.GetValueFromString<T>(value);
            }

            return propFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T, T>)doWhenChanged, doAfterNotify, GetComparerForCollections<T>(comparer, propFactory, useRefEquality));
        }

        // With No Value
        private static IProp<T> CreatePropWithNoValue<T>(AbstractPropFactory propFactory,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T, T>)doWhenChanged, doAfterNotify, GetComparerForCollections<T>(comparer, propFactory, useRefEquality));
        }

        private static Func<T, T, bool> GetComparerForProps<T>(Delegate comparer, IPropFactory propFactory, bool useRefEquality)
        {
            if (useRefEquality)
            {
                return propFactory.GetRefEqualityComparer<T>();
            }
            else if (comparer == null)
            {
                return null;
            }
            else
            {
                return (Func<T, T, bool>)comparer;
            }
        }
        #endregion
    }
}

