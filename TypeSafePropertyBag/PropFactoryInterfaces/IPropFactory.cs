using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public interface IPropFactory
    {
        #region Public Properties and Methods

        bool ProvidesStorage { get; }
        string IndexerName { get; }
        ResolveTypeDelegate TypeResolver { get; }
        IConvertValues ValueConverter { get; }

        IProvideDelegateCaches DelegateCacheProvider { get; }

        CViewProviderCreator GetCViewProviderFactory();
        BetterLCVCreatorDelegate<T> ListCollectionViewCreator<T>() where T: INotifyItemEndEdit;

        bool IsCollection(IProp prop);

        bool IsReadOnly(IProp prop);

        #endregion

        #region Enumerable-Type Prop Creation 

        #endregion

        #region ObservableCollection<T> Prop Creation

        ICProp<CT, T> Create<CT, T>(CT initialValue, PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool typeIsSolid,
            Func<CT, CT, bool> comparer) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        ICProp<CT, T> CreateWithNoValue<CT, T>(PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool typeIsSolid,
            Func<CT, CT, bool> comparer) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        #endregion

        #region CollectionViewSource Prop Creation

        IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider, IPropTemplate propTemplate);

        IProp CreateCVProp(PropNameType propertyName, IProvideAView viewProvider, IPropTemplate propTemplate);


        //IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider);

        //IProp CreateCVProp(PropNameType propertyName, IProvideAView viewProvider);

        #endregion

        #region DataSource Creation

        //CrudWithMapping<TSource, TDestination> Test<TSource, TDestination>();

        //ClrMappedDSP<TDestination> CreateMappedDS<TSource, TDestination>
        //    (
        //    PropIdType propId,
        //    PropKindEnum propKind,
        //    IDoCRUD<TSource> dal,
        //    PSAccessServiceInterface storeAccesor,
        //    IPropBagMapper<TSource, TDestination> mapper
        //    /*, out CrudWithMapping<TSource, TDestination> mappedDal*/
        //    )
        //    where TSource : class where TDestination : INotifyItemEndEdit;

        #endregion

        #region Scalar Prop Creation

        IProp<T> Create<T>(
            T value,
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool isTypeSolid,
            Func<T, T, bool> comparer,
            bool comparerIsRefEquality,
            Func<string, T> getDefaultValFunc);

        IProp<T> CreateWithNoValue<T>
            (
            PropNameType propertyName,
            object extraInfo,
            PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid,
            Func<T, T, bool> comparer,
            bool comparerIsRefEquality,
            Func<string, T> getDefaultValFunc
            );

        #endregion

        #region Generic Property Creation

        IProp CreateGenFromObject(Type typeOfThisProperty, object value, PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        //IProp CreateGenFromString(Type typeOfThisProperty, string value, bool useDefault, PropNameType propertyName, object extraInfo,
        //    PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
        //    Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        IProp CreateGenWithNoValue(Type typeOfThisProperty, PropNameType propertyName, object extraInfo,
            PropStorageStrategyEnum storageStrategy, bool isTypeSolid, PropKindEnum propKind,
            Delegate comparer, bool useRefEquality = false, Type collectionType = null);

        //IPropGen CreatePropInferType(object value, PropNameType propertyName, object extraInfo, bool hasStorage);

        //IProvideADataSourceProvider GetDSProviderProvider(PropIdType propId, PropKindEnum propKind, object iDoCrudDataSource, PSAccessServiceInterface storeAccesor, IMapperRequest mr);

        #endregion

        #region Default Value and Type Support

        Func<T, T, bool> GetRefEqualityComparer<T>();

        object GetDefaultValue(Type propertyType, PropNameType propertyName = null);

        T GetDefaultValue<T>(PropNameType propertyName = null);

        T GetValueFromObject<T>(object value);

        T GetValueFromString<T>(PropNameType value);

        CT GetValueFromString<CT, T>(PropNameType value) where CT : class;

        Type GetTypeFromValue(object value);

        bool IsTypeSolid(object value, Type propertyType);

        #endregion

        #region Diagnostics

        int DoSetCacheCount { get; }
        int CreateScalarPropCacheCount { get; }
        //int CreatePropWithNoValCacheCount { get; }

        #endregion
    }
}