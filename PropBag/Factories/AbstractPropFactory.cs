using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag
{

    public abstract class AbstractPropFactory : IPropFactory
    {
        public bool ReturnDefaultForUndefined { get; }
        public ResolveTypeDelegate TypeResolver { get; }

        public IConvertValues ValueConverter { get; }

        public abstract bool ProvidesStorage { get; }

        /// <summary>
        /// This is a const string property of System.Windows.Data.Binding,
        /// we are making it an overridable property in case other frameworks use something else.
        /// </summary>
        public virtual string IndexerName { get; }


        public AbstractPropFactory(bool returnDefaultForUndefined, 
            ResolveTypeDelegate typeResolver = null,
            IConvertValues valueConverter = null)
        {
            ReturnDefaultForUndefined = returnDefaultForUndefined;

            // Use our default implementation, if the caller did not supply one.
            TypeResolver = typeResolver ?? this.GetTypeFromName;

            // Use our default implementation, if the caller did not supply one.
            ValueConverter = valueConverter ?? new PropFactoryValueConverter();

            IndexerName = "Item[]";
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

        public abstract IProp<T> Create<T>(
            T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null);

        public abstract IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null);

        public abstract IPropGen CreateGenFromObject(Type typeOfThisProperty,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        public abstract IPropGen CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        public abstract IPropGen CreateGenWithNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);
        
        public virtual IPropGen CreatePropInferType(object value, string propertyName, object extraInfo, bool hasStorage)
        {
            System.Type typeOfThisValue;
            bool typeIsSolid;

            if (value == null)
            {
                typeOfThisValue = typeof(object);
                typeIsSolid = false;
            }
            else
            {
                // TODO, we probably need to be more creative when determining the type of this new value.
                typeOfThisValue = value.GetType();
                typeIsSolid = true;
            }

            IPropGen prop = this.CreateGenFromObject(typeOfThisValue, value, propertyName, extraInfo, 
                hasStorage, typeIsSolid, null, false, null, false);
            return prop;
        }

        /// <summary>
        /// Implementors of deriving classes must ensure that the value returned by this 
        /// method will be the same throughout the lifetime of the factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual Func<T,T,bool> GetRefEqualityComparer<T>()
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

        public virtual bool IsTypeSolid(object value, Type propertyType)
        {
            return (propertyType != typeof(object));
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

            Type s = value.GetType();

            // value is already of the correct type.
            if (s == t) return (T)(object)value;

            object parameter = new ControlModel.TwoTypes(t, s);

            return (T)ValueConverter.ConvertBack(value, t, parameter, CultureInfo.CurrentCulture);
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


        #region Shared Delegate Creation Logic

        //TODO: Consider caching the delegates that are created here.

        static private Type gmtType = typeof(APFGenericMethodTemplates);

        protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropFromObjectDelegate result = (CreatePropFromObjectDelegate)Delegate.CreateDelegate(typeof(CreatePropFromObjectDelegate), mi);

            return result;
        }

        protected virtual CreatePropFromStringDelegate GetPropFromStringCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropFromStringDelegate result = (CreatePropFromStringDelegate)Delegate.CreateDelegate(typeof(CreatePropFromStringDelegate), mi);

            return result;
        }

        protected virtual  CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropWithNoValueDelegate result = (CreatePropWithNoValueDelegate)Delegate.CreateDelegate(typeof(CreatePropWithNoValueDelegate), mi);

            return result;
        }

        #endregion
    }

    static class APFGenericMethodTemplates
    {
        private static IProp<T> CreatePropFromObject<T>(AbstractPropFactory propFactory,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            Func<T,T,bool> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (Func<T,T,bool>)comparer;

            T initVal = propFactory.GetValueFromObject<T>(value);

            return propFactory.Create<T>(initVal, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T,T>) doWhenChanged, doAfterNotify, compr);
        }

        private static IProp<T> CreatePropFromString<T>(AbstractPropFactory propFactory,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            Func<T, T, bool> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (Func<T, T, bool>)comparer;

            T initVal;
            if (useDefault)
            {
                initVal = propFactory.ValueConverter.GetDefaultValue<T>(propertyName);
            }
            else
            {
                initVal = propFactory.GetValueFromString<T>(value);
            }

            return propFactory.Create<T>(initVal, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T, T>)doWhenChanged, doAfterNotify, compr);
        }

        private static IProp<T> CreatePropWithNoValue<T>(AbstractPropFactory propFactory,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            Func<T,T,bool> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (Func<T,T,bool>)comparer;

            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid, 
                (Action<T, T>)doWhenChanged, doAfterNotify, compr);
        }


    }


}
