using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
     
    #region Enumerable-Type Methods

    #endregion

    #region IObsCollection<T> and ObservableCollection<T> Methods

    // From Object
    public delegate IProp CreateCPropFromObjectDelegate(IPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // From String
    public delegate IProp CreateCPropFromStringDelegate(IPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate IProp CreateCPropWithNoValueDelegate(IPropFactory propFactory,
        string propertyName, object extraInfo,
        PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    #endregion

    #region DataSource Methods

    public delegate IProvideADataSourceProvider CreateMappedDSPProviderDelegate(IPropFactory propFactory,
        PropIdType propId, PropKindEnum propKind, object genDal, 
        PSAccessServiceInterface propStoreAccessService, IPropBagMapperGen genMapper /* , out CrudWithMapping<TSource, TDestination> mappedDs*/);
        //where TSource : class where TDestination : INotifyItemEndEdit;

    #endregion

    #region CollectionView / CollectionViewSource Methods

    public delegate IProp CreateCVSPropDelegate(IPropFactory propFactory, string propertyName, IProvideAView viewProvider);
    public delegate IProp CreateCVPropDelegate(IPropFactory propFactory, string propertyName, IProvideAView viewProvider);

    #endregion

    #region Property-Type Methods

    // From Object
    public delegate IProp CreatePropFromObjectDelegate(IPropFactory propFactory,
        object value,
        string propertyName, object extraInfo,
        PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // From String
    public delegate IProp CreatePropFromStringDelegate(IPropFactory propFactory,
        string value, bool useDefault,
        string propertyName, object extraInfo,
        PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    // With No Value
    public delegate IProp CreatePropWithNoValueDelegate(IPropFactory propFactory,
        string propertyName, object extraInfo,
        PropStorageStrategyEnum storageStrategy, bool isTypeSolid,
        Delegate comparer, bool useRefEquality);

    #endregion
}
