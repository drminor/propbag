
using DRM.TypeSafePropertyBag.DataAccessSupport;
using DRM.TypeSafePropertyBag.Fundamentals;
using ObjectSizeDiagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public abstract class AbstractPropFactory : IPropFactory, IDisposable
    {
        #region Public Properties

        public ResolveTypeDelegate TypeResolver { get; }

        public IProvideDelegateCaches DelegateCacheProvider { get; }

        public virtual CViewProviderCreator GetCViewProviderFactory()
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)}" +
                $" does not provide a ViewProviderFactory, please use the WPFPropfactory or similar.");
        }

        public virtual PropBagMapperCreator GetPropBagMapperFactory()
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)}" +
                $" does not provide a PropBagMapperFactory, please use the WPFPropfactory or similar.");
        }

        public IConvertValues ValueConverter { get; }

        public abstract bool ProvidesStorage { get; }

        /// <summary>
        /// This is a const string property of System.Windows.Data.Binding,
        /// we are making it an overridable property in case other frameworks use something else.
        /// </summary>
        public virtual string IndexerName { get; }

        // These are used for diagnostics.
        public virtual int DoSetCacheCount => DelegateCacheProvider.DoSetDelegateCache.Count; // abstract int DoSetCacheCount { get; }
        public virtual int CreatePropFromStringCacheCount => DelegateCacheProvider.CreateScalarPropCache.Count; //abstract int CreatePropFromStringCacheCount { get; }
        //public virtual int CreatePropWithNoValCacheCount => DelegateCacheProvider.CreatePropWithNoValCache.Count; //abstract int CreatePropWithNoValCacheCount { get; }

        #endregion

        #region Constructor

        public AbstractPropFactory
            (
            IProvideDelegateCaches delegateCacheProvider,
            IConvertValues valueConverter,
            ResolveTypeDelegate typeResolver)
        {
            DelegateCacheProvider = delegateCacheProvider ?? throw new ArgumentNullException(nameof(delegateCacheProvider));
            ValueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));

            if (typeResolver != null)
            {
                TypeResolver = typeResolver;
            }
            else
            {
                // Use our default implementation, if the caller did not supply one.
                TypeResolver = new SimpleTypeResolver().GetTypeFromName;
            }
            ////TypeResolver = typeResolver ??  this.GetTypeFromName;

            //// Temporarily, for testing, always use our implementation.
            //TypeResolver = new SimpleTypeResolver().GetTypeFromName;

            IndexerName = "Item[]";
        }

        #endregion

        #region Prop Support

        public virtual bool IsCollection(IProp prop)
        {
            //return IsCollection(prop.PropKind);
            return prop.PropKind.IsCollection();
        }

        //public virtual bool IsCollection(PropKindEnum propKind)
        //{
        //    if (propKind == PropKindEnum.Prop)
        //    {
        //        return false;
        //    }
        //    else if
        //        (
        //        propKind == PropKindEnum.ObservableCollection ||
        //        propKind == PropKindEnum.EnumerableTyped ||
        //        propKind == PropKindEnum.Enumerable ||
        //        propKind == PropKindEnum.ObservableCollection_RO ||
        //        propKind == PropKindEnum.EnumerableTyped_RO ||
        //        propKind == PropKindEnum.Enumerable_RO
        //        )
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        CheckPropKindSpecial(propKind);
        //        return false;
        //    }
        //}

        public virtual bool IsReadOnly(IProp prop)
        {
            //return IsReadOnly(prop.PropKind);
            return prop.PropKind.IsReadOnly();

        }

        //public virtual bool IsReadOnly(PropKindEnum propKind)
        //{
        //    if (propKind == PropKindEnum.Prop)
        //    {
        //        return false;
        //    }
        //    else if
        //        (
        //        propKind == PropKindEnum.CollectionViewSource_RO ||
        //        propKind == PropKindEnum.ObservableCollection_RO ||
        //        propKind == PropKindEnum.EnumerableTyped_RO ||
        //        propKind == PropKindEnum.Enumerable_RO
        //        )
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        CheckPropKind(propKind);
        //        return false;
        //    }
        //}

        #endregion

        #region Enumerable-Type Prop Creation 

        #endregion

        #region ObservableCollection<T> Prop Creation

        public abstract ICProp<CT, T> Create<CT, T>(
            CT initialValue,
            PropNameType propertyName, object extraInfo = null,
            PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
            Func<CT, CT, bool> comparer = null) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        //{
        //    if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
        //    GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

        //    ICProp<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage, comparer);
        //    return prop;
        //}

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

        public virtual IProp CreateCVProp(PropNameType propertyName, IProvideAView viewProvider)
        {
            throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVProps (CollectionView PropItems), please use WPFPropfactory or similar.");
        }

        #endregion

        #region DataSource creation

        //public abstract ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>(
        //    IDoCRUD<TSource> dal,
        //    IPropBagMapper<TSource, TDestination> mapper
        //    /*, out CrudWithMapping<TSource, TDestination> mappedDal*/) where TSource : class where TDestination : INotifyItemEndEdit;

        public abstract ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>(PropIdType propId, PropKindEnum propKind,
            IDoCRUD<TSource> dal, PSAccessServiceInterface storeAccesor, IPropBagMapper<TSource, TDestination> mapper
            /*, out CrudWithMapping<TSource, TDestination> mappedDal*/) where TSource : class where TDestination : INotifyItemEndEdit;


        #endregion

        #region Scalar Prop Creation

        public abstract IProp<T> Create<T>(
            bool haveValue,
            T initialValue,
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            GetDefaultValueDelegate<T> getDefaultValFunc);

        //public abstract IProp<T> Create_OLD<T>(
        //    T initialValue,
        //    PropNameType propertyName, object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        //    Func<T, T, bool> comparer = null);
        ////{
        ////    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        ////    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
        ////    IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
        ////    return prop;
        ////}

        public abstract IProp<T> CreateWithNoValue<T>
            (
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            GetDefaultValueDelegate<T> getDefaultValFunc
            );


        //public abstract IProp<T> CreateWithNoValue<T>(
        //    PropNameType propertyName, object extraInfo = null,
        //    PropStorageStrategyEnum storageStrategy = PropStorageStrategyEnum.Internal, bool typeIsSolid = true,
        //    Func<T, T, bool> comparer = null);
        ////{
        ////    if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

        ////    GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>;
        ////    IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid, hasStore: hasStorage, comparer: comparer);
        ////    return prop;
        ////}

        #endregion

        #region Generic Property Creation

        public abstract IProvideADataSourceProvider GetDSProviderProvider(PropIdType propId, PropKindEnum propKind, object iDoCrudDataSource, PSAccessServiceInterface storeAccesor, IMapperRequest mr);

        public virtual IProp CreateGenFromObject(Type typeOfThisProperty,
            object value,
            PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            MemConsumptionTracker mct = new MemConsumptionTracker(enabled: false);

            if (propKind == PropKindEnum.Prop)
            {
                CreateScalarProp propCreator = GetPropCreator(typeOfThisProperty);
                mct.MeasureAndReport("GetPropCreator", $"for {propertyName}");

                IProp prop = propCreator(this, true, value, false, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality, getDefaultValFunc: null);

                mct.MeasureAndReport("Ran propCreator to get IProp", $"for {propertyName}");

                return prop;
            }
            else if (propKind.IsCollection())
            {
                CreateCPropFromObjectDelegate propCreator = GetCPropCreator(typeOfThisProperty, itemType);
                mct.MeasureAndReport("GetCPropCreator", $"for {propertyName}");

                IProp prop = propCreator(this, value, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
                mct.MeasureAndReport("Ran GetCPropCreator to get IProp", $"for {propertyName}");

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
            MemConsumptionTracker mct = new MemConsumptionTracker(enabled: false);

            if (propKind == PropKindEnum.Prop)
            {
                //CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
                //mct.MeasureAndReport("GetPropFromStringCreator", $"for {propertyName}");

                //IProp prop = propCreator(this, value, useDefault, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                //    comparer: comparer, useRefEquality: useRefEquality);

                CreateScalarProp propCreator = GetPropCreator(typeOfThisProperty);
                mct.MeasureAndReport("GetPropCreator", $"for {propertyName}");

                IProp prop = propCreator(this, true, value, useDefault, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality, getDefaultValFunc: null);

                mct.MeasureAndReport("Ran propCreator to get IProp", $"for {propertyName}");

                return prop;
            }
            else if (propKind.IsCollection())
            {
                CreateCPropFromStringDelegate propCreator = GetCPropFromStringCreator(typeOfThisProperty, itemType);
                mct.MeasureAndReport("GetCPropFromStringCreator", $"for {propertyName}");

                IProp prop = propCreator(this, value, useDefault, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);
                mct.MeasureAndReport("Ran GetCPropFromStringCreator to get IProp", $"for {propertyName}");

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
            MemConsumptionTracker mct = new MemConsumptionTracker(enabled: false);

            if (propKind == PropKindEnum.Prop)
            {
                //CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
                //IProp prop = propCreator(this, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                //    comparer: comparer, useRefEquality: useRefEquality);

                CreateScalarProp propCreator = GetPropCreator(typeOfThisProperty);
                mct.MeasureAndReport("GetPropCreator", $"for {propertyName}");

                IProp prop = propCreator(this, false, null, false, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality, getDefaultValFunc: null);

                mct.MeasureAndReport("Ran propCreator to get IProp", $"for {propertyName}");

                return prop;
            }
            else if (propKind.IsCollection())
            {
                CreateCPropWithNoValueDelegate propCreator = GetCPropWithNoValueCreator(typeOfThisProperty, itemType);
                IProp prop = propCreator(this, propertyName, extraInfo, storageStrategy: storageStrategy, isTypeSolid: isTypeSolid,
                    comparer: comparer, useRefEquality: useRefEquality);

                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        //// TODO: Update execption message to inform caller that these properties can be created by calls made
        //// directly to the IPropBag instance, since no generic type parameters are required.
        //public virtual IProp CreateCVSPropFromString(Type typeOfThisProperty, PropNameType propertyName, IProvideAView viewProvider)
        //{
        //    throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVSProps (CollectionViewSource PropItems), please use WPFPropfactory or similar.");
        //}

        //public virtual IProp CreateCVPropFromString(Type typeofThisProperty, string propertyName, IProvideAView viewProvider)
        //{
        //    throw new NotImplementedException($"This implementation of {nameof(IPropFactory)} cannot create CVProps (CollectionView PropItems), please use WPFPropfactory or similar.");
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
            else if (value == string.Empty)
            {
                return default(CT);
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

        #region ObservableCollection<T> Prop Creation Methods

        // From Object
        protected virtual CreateCPropFromObjectDelegate GetCPropCreator(Type collectionType, Type itemType)
        {
            CreateCPropFromObjectDelegate result = DelegateCacheProvider.CreateCPropFromObjectCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;
        }

        // From String
        protected virtual CreateCPropFromStringDelegate GetCPropFromStringCreator(Type collectionType, Type itemType)
        {
            CreateCPropFromStringDelegate result = DelegateCacheProvider.CreateCPropFromStringCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;
        }

        // With No Value
        protected virtual CreateCPropWithNoValueDelegate GetCPropWithNoValueCreator(Type collectionType, Type itemType)
        {
            CreateCPropWithNoValueDelegate result = DelegateCacheProvider.CreateCPropWithNoValCache.GetOrAdd(new TypePair(collectionType, itemType));
            return result;
        }

        #endregion


        #region Create DataSourceProvider-Providers

        //From Object
        //protected virtual CreateDsFromIDoCrudDelegate GetCPropCreator(Type collectionType, Type itemType)
        //{
        //    CreateEPropFromObjectDelegate result = DelegateCacheProvider.CreateCPropFromObjectCache.GetOrAdd(new TypePair(collectionType, itemType));
        //    return result;
        //}

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

        #region DataSource Creation Methods

        protected virtual CreateMappedDSPProviderDelegate GetDSPProviderCreator(Type sourceType, Type destinationType)
        {
            CreateMappedDSPProviderDelegate result = DelegateCacheProvider.CreateDSPProviderCache.GetOrAdd(new TypePair(sourceType, destinationType));
            return result;
        }

        #endregion

        #region Scalar Prop Creation Methods

        // From Object
        protected virtual CreateScalarProp GetPropCreator(Type typeOfThisValue)
        {
            CreateScalarProp result = DelegateCacheProvider.CreateScalarPropCache.GetOrAdd(typeOfThisValue);
            return result;
        }

        //// From Object
        //protected virtual CreatePropFromObjectDelegate GetPropCreator(Type typeOfThisValue)
        //{
        //    CreatePropFromObjectDelegate result = DelegateCacheProvider.CreatePropFromObjectCache.GetOrAdd(typeOfThisValue);
        //    return result;
        //}

        //// From String
        //protected virtual CreatePropFromStringDelegate GetPropFromStringCreator(Type typeOfThisValue)
        //{
        //    CreatePropFromStringDelegate result = DelegateCacheProvider.CreateScalarPropCache.GetOrAdd(typeOfThisValue);
        //    return result;
        //}

        //// With No Value
        //protected virtual CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        //{
        //    CreatePropWithNoValueDelegate result = DelegateCacheProvider.CreatePropWithNoValCache.GetOrAdd(typeOfThisValue);
        //    return result;
        //}

        #endregion Property-Type Methods

        #endregion Shared Delegate Creation

        //#region DEBUG Checks

        //[System.Diagnostics.Conditional("DEBUG")]
        //private void CheckPropKind(PropKindEnum propKind)
        //{
        //    if (!
        //            (
        //            propKind == PropKindEnum.CollectionView ||
        //            propKind == PropKindEnum.CollectionViewSource ||
        //            propKind == PropKindEnum.CollectionViewSource_RO ||
        //            propKind == PropKindEnum.DataTable ||
        //            propKind == PropKindEnum.DataTable_RO ||
        //            propKind == PropKindEnum.Prop ||
        //            propKind == PropKindEnum.ObservableCollection ||
        //            propKind == PropKindEnum.EnumerableTyped ||
        //            propKind == PropKindEnum.Enumerable ||
        //            propKind == PropKindEnum.ObservableCollection_RO ||
        //            propKind == PropKindEnum.EnumerableTyped_RO ||
        //            propKind == PropKindEnum.Enumerable_RO
        //            )
        //        )
        //    {
        //        throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
        //    }
        //}

        //[System.Diagnostics.Conditional("DEBUG")]
        //private void CheckPropKindSpecial(PropKindEnum propKind)
        //{
        //    if (!
        //            (
        //            propKind == PropKindEnum.CollectionView ||
        //            propKind == PropKindEnum.CollectionViewSource ||
        //            propKind == PropKindEnum.CollectionViewSource_RO ||
        //            propKind == PropKindEnum.DataTable ||
        //            propKind == PropKindEnum.DataTable_RO
        //            )
        //        )
        //    {
        //        throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
        //    }
        //}

        //#endregion 

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //_propStoreAccessServiceProvider.Dispose();
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

