
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    public abstract class AbstractPropFactory : IPropFactory, IDisposable
    {
        PSAccessServiceProviderType _propStoreAccessServiceProvider { get; }

        #region Public Properties

        public ResolveTypeDelegate TypeResolver { get; }

        public IProvideDelegateCaches DelegateCacheProvider { get; }

        public virtual CViewProviderCreator CViewProviderFactory =>
                        throw new NotImplementedException($"This implementation of {nameof(IPropFactory)}" +
                            $" does not provide a ViewProviderFactory, please use WPFPropfactory or similar.");


        public IConvertValues ValueConverter { get; }

        public abstract bool ProvidesStorage { get; }

        /// <summary>
        /// This is a const string property of System.Windows.Data.Binding,
        /// we are making it an overridable property in case other frameworks use something else.
        /// </summary>
        public virtual string IndexerName { get; }

        // These are used for diagnostics.
        public virtual int DoSetCacheCount => DelegateCacheProvider.DoSetDelegateCache.Count; // abstract int DoSetCacheCount { get; }
        public virtual int CreatePropFromStringCacheCount => DelegateCacheProvider.CreatePropFromStringCache.Count; //abstract int CreatePropFromStringCacheCount { get; }
        public virtual int CreatePropWithNoValCacheCount => DelegateCacheProvider.CreatePropWithNoValCache.Count; //abstract int CreatePropWithNoValCacheCount { get; }

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

            _propStoreAccessServiceProvider = propStoreAccessServiceProvider ?? throw new ArgumentNullException(nameof(propStoreAccessServiceProvider));
            DelegateCacheProvider = delegateCacheProvider;

            // Use our default implementation, if the caller did not supply one.
            TypeResolver = typeResolver ?? this.GetTypeFromName;

        // Use our default implementation, if the caller did not supply one.
        ValueConverter = valueConverter; //?? new PropFactoryValueConverter(delegateCacheProvider.TypeDescBasedTConverterCache);

            IndexerName = "Item[]";
        }

        #endregion

        #region Prop Support

        public virtual bool IsCollection(IProp prop)
        {
            return IsCollection(prop.PropKind);
        }

        public virtual bool IsCollection(PropKindEnum propKind)
        {
            if (propKind == PropKindEnum.Prop)
            {
                return false;
            }
            else if
                (
                propKind == PropKindEnum.ObservableCollection ||
                //propKind == PropKindEnum.ObservableCollectionFB ||
                propKind == PropKindEnum.EnumerableTyped ||
                propKind == PropKindEnum.Enumerable ||
                propKind == PropKindEnum.ObservableCollection_RO ||
                //propKind == PropKindEnum.ObservableCollectionFB_RO ||
                propKind == PropKindEnum.EnumerableTyped_RO ||
                propKind == PropKindEnum.Enumerable_RO
                )
            {
                return true;
            }
            else
            {
                CheckPropKindSpecial(propKind);
                return false;
            }
        }

        public virtual bool IsReadOnly(IProp prop)
        {
            return IsReadOnly(prop.PropKind);
        }

        public virtual bool IsReadOnly(PropKindEnum propKind)
        {
            if (propKind == PropKindEnum.Prop)
            {
                return false;
            }
            else if
                (
                propKind == PropKindEnum.CollectionViewSource_RO ||
                propKind == PropKindEnum.ObservableCollection_RO ||
                //propKind == PropKindEnum.ObservableCollectionFB_RO ||
                propKind == PropKindEnum.EnumerableTyped_RO ||
                propKind == PropKindEnum.Enumerable_RO
                )
            {
                return true;
            }
            else
            {
                CheckPropKind(propKind);
                return false;
            }
        }

        #endregion

        #region PropStore Support

        public PSAccessServiceType CreatePropStoreService(IPropBagInternal propBag)
        {
            PSAccessServiceType result = _propStoreAccessServiceProvider.CreatePropStoreService(propBag);
            return result;
        }

        #endregion

        #region Enumerable-Type Prop Creation 

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation

        public abstract ICProp<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        //{
        //    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
        //    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        //    ICProp<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        //    return prop;
        //}

        //public abstract ICPropFB<CT, T> CreateFB<CT, T>(
        //    CT initialValue,
        //    string propertyName, object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        //    Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        ////{
        ////    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
        ////    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        ////    ICPropFB<CT, T> prop = new CPropFB<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        ////    return prop;
        ////}

        public abstract ICProp<CT, T> CreateWithNoValue<CT, T>(
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        //{
        //    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

        //    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        //    ICProp<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        //    return prop;
        //}

        #endregion

        #region CollectionViewSource Prop Creation

        public virtual IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider) 
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVSProps (CollectionViewSource PropItems), please use WPFPropfactory or similar.");
        }

        public virtual IProp CreateCVProp(string propertyName, IProvideAView viewProvider)
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVProps (CollectionView PropItems), please use WPFPropfactory or similar.");
        }


        #endregion

        #region Scalar Prop Creation

        public abstract IProp<T> Create<T>(
            T initialValue,
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null);
        //{
        //    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        //    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
        //    IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
        //    return prop;
        //}

        public abstract IProp<T> CreateWithNoValue<T>(
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<T, T, bool> comparer = null);
        //{
        //    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        //    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
        //    IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
        //    return prop;
        //}

        #endregion

        #region Generic Property Creation

        public virtual IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromObjectDelegate propCreator = GetPropCreator(typeOfThisProperty);
                IProp prop = propCreator(this, value, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else if (IsCollection(propKind))
            {
                CreateEPropFromObjectDelegate propCreator = GetCPropCreator(typeOfThisProperty, itemType);
                IProp prop = propCreator(this, value, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public virtual IProp CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
                IProp prop = propCreator(this, value, useDefault, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else if (IsCollection(propKind))
            {
                CreateEPropFromStringDelegate propCreator = GetCPropFromStringCreator(typeOfThisProperty, itemType);
                IProp prop = propCreator(this, value, useDefault, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public virtual IProp CreateGenWithNoValue(Type typeOfThisProperty,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
                IProp prop = propCreator(this, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else if (IsCollection(propKind))
            {
                CreateEPropWithNoValueDelegate propCreator = GetCPropWithNoValueCreator(typeOfThisProperty, itemType);
                IProp prop = propCreator(this, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public virtual IProp CreateCVSPropFromString(Type typeOfThisProperty, PropNameType propertyName, IProvideAView viewProvider)
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVSProps (CollectionViewSource PropItems), please use WPFPropfactory or similar.");
        }

        public virtual IProp CreateCVPropFromString(Type typeofThisProperty, string propertyName, IProvideAView viewProvider)
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVProps (CollectionView PropItems), please use WPFPropfactory or similar.");
        }

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

            object parameter = new TwoTypes(t, vType);

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

            object parameter = new TwoTypes(t, s);

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

        #region Enumerable-Type Prop Creation Methods

        #endregion

        #region IObsCollection<T> and ObservableCollection<T> Prop Creation Methods

        // From Object
        protected virtual CreateEPropFromObjectDelegate GetCPropCreator(Type collectionType, Type itemType)
        {
            CreateEPropFromObjectDelegate result = DelegateCacheProvider.CreateCPropFromObjectCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;
        }

        // From String
        protected virtual CreateEPropFromStringDelegate GetCPropFromStringCreator(Type collectionType, Type itemType)
        {
            CreateEPropFromStringDelegate result = DelegateCacheProvider.CreateCPropFromStringCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;
        }

        //// From String FALL BACK to ObservableCollection<T>
        //protected virtual CreateEPropFromStringDelegate GetCPropFromStringFBCreator(Type collectionType, Type itemType)
        //{
        //    CreateEPropFromStringDelegate result = DelegateCacheProvider.CreateCPropFromStringFBCache.GetOrAdd(new TypePair(collectionType, itemType));
        //    return result;
        //}

        // With No Value
        protected virtual CreateEPropWithNoValueDelegate GetCPropWithNoValueCreator(Type collectionType, Type itemType)
        {
            CreateEPropWithNoValueDelegate result = DelegateCacheProvider.CreateCPropWithNoValCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;
        }

        #endregion

        #region CollectionViewSource Prop Creation Methods

        //// CollectionViewSource
        //protected virtual CreateCVSPropDelegate GetCVSPropCreator(Type itemType)
        //{
        //    CreateCVSPropDelegate result = DelegateCacheProvider.CreateCVSPropCache.GetOrAdd(itemType);
        //    return result;
        //}

        //// CollectionView
        //protected virtual CreateCVPropDelegate GetCVPropCreator(Type itemType)
        //{
        //    CreateCVPropDelegate result = DelegateCacheProvider.CreateCVPropCache.GetOrAdd(itemType);
        //    return result;
        //}

        #endregion

        #region Scalar Prop Creation Methods

        // From Object
        protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        {
            CreatePropFromObjectDelegate result = DelegateCacheProvider.CreatePropFromObjectCache.GetOrAdd(typeOfThisValue);
            return result;
        }

        // From String
        protected virtual CreatePropFromStringDelegate GetPropFromStringCreator(Type typeOfThisValue)
        {
            CreatePropFromStringDelegate result = DelegateCacheProvider.CreatePropFromStringCache.GetOrAdd(typeOfThisValue);
            return result;
        }

        // With No Value
        protected virtual CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            CreatePropWithNoValueDelegate result = DelegateCacheProvider.CreatePropWithNoValCache.GetOrAdd(typeOfThisValue);
            return result;
        }

        #endregion Property-Type Methods

        #endregion Shared Delegate Creation

        #region DEBUG Checks

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckPropKind(PropKindEnum propKind)
        {
            if (!
                    (
                    propKind == PropKindEnum.CollectionView ||
                    propKind == PropKindEnum.CollectionViewSource ||
                    propKind == PropKindEnum.CollectionViewSource_RO ||
                    propKind == PropKindEnum.DataTable ||
                    propKind == PropKindEnum.DataTable_RO ||
                    propKind == PropKindEnum.Prop ||
                    propKind == PropKindEnum.ObservableCollection ||
                    //propKind == PropKindEnum.ObservableCollectionFB ||
                    propKind == PropKindEnum.EnumerableTyped ||
                    propKind == PropKindEnum.Enumerable ||
                    propKind == PropKindEnum.ObservableCollection_RO ||
                    //propKind == PropKindEnum.ObservableCollectionFB_RO ||
                    propKind == PropKindEnum.EnumerableTyped_RO ||
                    propKind == PropKindEnum.Enumerable_RO
                    )
                )
            {
                throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckPropKindSpecial(PropKindEnum propKind)
        {
            if (!
                    (
                    propKind == PropKindEnum.CollectionView ||
                    propKind == PropKindEnum.CollectionViewSource ||
                    propKind == PropKindEnum.CollectionViewSource_RO ||
                    propKind == PropKindEnum.DataTable ||
                    propKind == PropKindEnum.DataTable_RO
                    )
                )
            {
                throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
            }
        }

        #endregion 

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _propStoreAccessServiceProvider.Dispose();
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

