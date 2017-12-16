using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;

    public abstract class AbstractPropFactory : IPropFactory, IDisposable
    {
        #region Public Properties

        public ResolveTypeDelegate TypeResolver { get; }

        public IProvideDelegateCaches DelegateCacheProvider { get; }

        public IConvertValues ValueConverter { get; }

        public abstract bool ProvidesStorage { get; }

        /// <summary>
        /// This is a const string property of System.Windows.Data.Binding,
        /// we are making it an overridable property in case other frameworks use something else.
        /// </summary>
        public virtual string IndexerName { get; }

        public PSAccessServiceProviderType PropStoreAccessServiceProvider { get; }

        public abstract int DoSetCacheCount { get; }
        public abstract int CreatePropFromStringCacheCount { get; }
        public abstract int CreatePropWithNoValCacheCount { get; }

        #endregion

        #region Constructor

        public AbstractPropFactory
            (
            PSAccessServiceProviderType propStoreAccessServiceProvider,
            IProvideDelegateCaches delegateCacheProvider,
            ResolveTypeDelegate typeResolver = null,
            IConvertValues valueConverter = null
            )
        {

            PropStoreAccessServiceProvider = propStoreAccessServiceProvider ?? throw new ArgumentNullException(nameof(propStoreAccessServiceProvider));
            DelegateCacheProvider = delegateCacheProvider;

            // Use our default implementation, if the caller did not supply one.
            TypeResolver = typeResolver ?? this.GetTypeFromName;

            // Use our default implementation, if the caller did not supply one.
            ValueConverter = valueConverter ?? new PropFactoryValueConverter(delegateCacheProvider.TypeDescBasedTConverterCache);

            IndexerName = "Item[]";
        }

        #endregion

        #region Collection-type property creators

        public abstract ICPropPrivate<CT, T> Create<CT, T>(
            CT initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        public abstract ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : IEnumerable<T>;

        #endregion

        #region Propety-type property creators

        public abstract IProp<T> Create<T>(
            T initialValue,
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null);

        public abstract IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null);

        #endregion

        #region Generic property creators

        public abstract IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null);

        public abstract IProp CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null);

        public abstract IProp CreateGenWithNoValue(Type typeOfThisProperty,
            PropNameType propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null);

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
            RefEqualityComparer<T> x = RefEqualityComparer<T>.Default;
            return x.Equals;
        }

        public T GetDefaultValue<T>(PropNameType propertyName = null)
        {
            return ValueConverter.GetDefaultValue<T>(propertyName);
            //return default(T);
        }

        public object GetDefaultValue(Type propertyType, PropNameType propertyName = null)
        {
            return ValueConverter.GetDefaultValue(propertyType, propertyName);
            //if (propertyType == null)
            //{
            //    throw new InvalidOperationException($"Cannot manufacture a default value if the type is specified as null for property: {propertyName}.");
            //}

            //if (propertyType == typeof(PropNameType))
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

        //static private Type gmtType = typeof(APFGenericMethodTemplates);

        #region Collection-Type Methods

        // From Object
        protected virtual CreateCPropFromObjectDelegate GetCPropCreator(Type collectionType, Type itemType)
        {
            CreateCPropFromObjectDelegate result = DelegateCacheProvider.CreateCPropFromObjectCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;

            //MethodInfo mi = gmtType.GetMethod("CreateCPropFromObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);
            //CreateCPropFromObjectDelegate result = (CreateCPropFromObjectDelegate)Delegate.CreateDelegate(typeof(CreateCPropFromObjectDelegate), mi);
            //return result;
        }

        // From String
        protected virtual CreateCPropFromStringDelegate GetCPropFromStringCreator(Type collectionType, Type itemType)
        {
            CreateCPropFromStringDelegate result = DelegateCacheProvider.CreateCPropFromStringCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;

            //MethodInfo mi = gmtType.GetMethod("CreateCPropFromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);
            //CreateCPropFromStringDelegate result = (CreateCPropFromStringDelegate)Delegate.CreateDelegate(typeof(CreateCPropFromStringDelegate), mi);
            //return result;
    }

        // With No Value
        protected virtual CreateCPropWithNoValueDelegate GetCPropWithNoValueCreator(Type collectionType, Type itemType)
        {
            CreateCPropWithNoValueDelegate result = DelegateCacheProvider.CreateCPropWithNoValCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;

            //MethodInfo mi = gmtType.GetMethod("CreateCPropWithNoValue",BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(collectionType, itemType);
            //CreateCPropWithNoValueDelegate result = (CreateCPropWithNoValueDelegate)Delegate.CreateDelegate(typeof(CreateCPropWithNoValueDelegate), mi);
            //return result;
        }

        #endregion

        #region Property-Type Methods

        // From Object
        protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        {
            CreatePropFromObjectDelegate result = DelegateCacheProvider.CreatePropFromObjectCache.GetOrAdd(typeOfThisValue);
            return result;

            //MethodInfo mi = gmtType.GetMethod("CreatePropFromObject", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            //CreatePropFromObjectDelegate result = (CreatePropFromObjectDelegate)Delegate.CreateDelegate(typeof(CreatePropFromObjectDelegate), mi);
            //return result;
        }

        // From String
        protected virtual CreatePropFromStringDelegate GetPropFromStringCreator(Type typeOfThisValue)
        {

            CreatePropFromStringDelegate result = DelegateCacheProvider.CreatePropFromStringCache.GetOrAdd(typeOfThisValue);
            return result;

            //MethodInfo mi = gmtType.GetMethod("CreatePropFromString", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            //CreatePropFromStringDelegate result = (CreatePropFromStringDelegate)Delegate.CreateDelegate(typeof(CreatePropFromStringDelegate), mi);
            //return result;
        }

        // With No Value
        protected virtual CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            CreatePropWithNoValueDelegate result = DelegateCacheProvider.CreatePropWithNoValCache.GetOrAdd(typeOfThisValue);
            return result;

            //MethodInfo mi = gmtType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            //CreatePropWithNoValueDelegate result = (CreatePropWithNoValueDelegate)Delegate.CreateDelegate(typeof(CreatePropWithNoValueDelegate), mi);
            //return result;
        }

        #endregion Property-Type Methods

        #endregion Shared Delegate Creation

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    PropStoreAccessServiceProvider.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}

