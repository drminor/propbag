using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using ObjectIdType = UInt64;
    using PropItemSetKeyType = PropItemSetKey<String>;

    public interface IPropStoreAccessService<L2T, L2TRaw> : IRegisterSubscriptions<L2T>, IRegisterBindings<L2T>, IDisposable
    {
        ObjectIdType ObjectId { get; }
        int PropertyCount { get; }

        bool TryGetPropId(L2TRaw propertyName, out L2T propId);
        bool TryGetPropName(L2T propertyId, out L2TRaw propertyName);

        bool IsPropItemSetFixed { get; }
        //object FixPropItemSet();
        //bool TryOpenPropItemSet(out object propItemSet_Handle);

        bool TryFixPropItemSet(PropItemSetKey<L2TRaw> propItemSetKey);
        bool TryOpenPropItemSet(/*out object propItemSet_Handle*/);

        // IDictionary-Like Methods
        IPropData this[IPropBag propBag, L2T propId] { get; }

        IPropStoreFastAccess<L2T, L2TRaw> GetFastAccessService();

        object GetValueFast(IPropBag component, L2T propId, PropItemSetKey<L2TRaw> propItemSetKey);
        bool SetValueFast(IPropBag component, L2T propId, PropItemSetKey<L2TRaw> propItemSetKey, object value);

        object GetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey);
        bool SetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey, object value);

        bool ContainsKey(IPropBag propBag, L2T propId);

        IReadOnlyDictionary<L2TRaw, IPropData> GetCollection(IPropBag propBag);
        //IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropData>> GetEnumerator(IPropBag propBag);

        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropData> GetValues(IPropBag propBag);
        IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetPropDataItemsWithNames(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2TRaw propertyName, IProp genericTypedProp, out IPropData propData, out L2T propId);

        bool TryAdd(IPropBag propBag, L2TRaw propertyName, IProp genericTypedProp,
            EventHandler<PcGenEventArgs> handler, SubscriptionPriorityGroup priorityGroup,
            out IPropData propData, out L2T propId);

        bool TryAdd<PropT>(IPropBag propBag, L2TRaw propertyName, IProp genericTypedProp,
            EventHandler<PcTypedEventArgs<PropT>> handler, SubscriptionPriorityGroup priorityGroup,
            out IPropData propData, out L2T propId);

        bool TryAdd(IPropBag propBag, L2TRaw propertyName, IProp genericTypedProp,
            object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup,
            out IPropData propData, out L2T propId);

        bool TryGetValue(IPropBag propBag, L2T propId, out IPropData propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropData propData);

        // Restricted Update Method on Dictionary-like object.
        bool SetTypedProp(IPropBag propBag, L2T propId, L2TRaw propertyName, IProp genericTypedProp);

        IPropStoreAccessServiceCreator<L2T, L2TRaw> StoreAcessorCreator { get; }

        IPropStoreAccessService<L2T, L2TRaw> CloneProps(IPropBag callingPropBag, IPropBag copySource);
        int ClearAllProps(IPropBag propBag);

        // Collection View Related
        //IProvideADataSourceProvider GetDataSourceProviderProvider(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder);

        IManageCViews GetViewManager(IPropBag propBag, L2T propId);

        DataSourceProvider GetDataSourceProvider(IPropBag propBag, L2T propId);

        DataSourceProvider GetOrAddDataSourceProvider(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder);

        IManageCViews GetOrAddViewManager(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder);
        IManageCViews GetOrAddViewManager(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder, IProvideADataSourceProvider dSProviderProvider);

        //// Providing a Mapper directly
        //IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        //    (
        //    IPropBag propBag, // The client to this PropStore Access Service
        //    L2T propId, // Identifies the PropItem that implements IDoCrud<TSource>
        //    IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
        //    IPropBagMapper<TSource, TDestination> mapper,
        //    CViewProviderCreator viewBuilder, // Method that can be used to create a IProvideAView from a DataSourceProvider.
        //    BetterLCVCreatorDelegate<TDestination> betterLCVCreatorDelegate
        //    )
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag;

        //// Using a IMapperRequest and Factory.
        //IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
        //    (
        //    IPropBag propBag, // The client to this PropStore Access Service
        //    L2T propId, // Identifies the PropItem that implements IDoCrud<TSource>
        //    IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
        //    IMapperRequest mr, // The information necessar to create a IPropBagMapper<TSource, TDestination>
        //    PropBagMapperCreator propBagMapperCreator, // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
        //    CViewProviderCreator viewBuilder, // Method that can be used to create a IProvideAView from a DataSourceProvider.
        //    BetterLCVCreatorDelegate<TDestination> betterLCVCreatorDelegate
        //    )
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag;

        // Using a CrudWithMappingCreator.
        IManageCViews GetOrAddViewManager_New<TDal, TSource, TDestination>
            (
            IPropBag propBag, // The client to this PropStore Access Service
            L2T propId, // Identifies the PropItem that implements IDoCrud<TSource>
            IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
            IMapperRequest mr, // The information necessar to create a IPropBagMapper<TSource, TDestination>

            CrudWithMappingCreator<TDal, TSource, TDestination> crudWithMappingCreator,

            CViewProviderCreator viewBuilder, // Method that can be used to create a IProvideAView from a DataSourceProvider.
            BetterLCVCreatorDelegate<TDestination> betterLCVCreatorDelegate
            )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
                        where TDestination : class, INotifyItemEndEdit, IPropBag;


        //IProvideATypedCViewManager<EndEditWrapper<TDestination>, TDestination> GetOrAddViewManagerProviderTyped<TDal, TSource, TDestination>
        //    (
        //        IPropBag propBag,   // The client of this service.
        //        IViewManagerProviderKey viewManagerProviderKey,
        //        PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
        //        CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        //    )
        //        where TDal : class, IDoCRUD<TSource>
        //        where TSource : class
        //                    where TDestination : class, INotifyItemEndEdit, IPropBag;

        //IProvideACViewManager GetOrAddViewManagerProvider<TDal, TSource, TDestination>
        //(
        //    IPropBag propBag,   // The client of this service.
        //    IViewManagerProviderKey viewManagerProviderKey,
        //    //LocalBindingInfo bindingInfo,
        //    //IMapperRequest mr,  // The information necessary to create a IPropBagMapper<TSource, TDestination>
        //    PropBagMapperCreator propBagMapperCreator,  // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
        //    CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        //)
        //    where TDal : class, IDoCRUD<TSource>
        //    where TSource : class
        //                where TDestination : class, INotifyItemEndEdit, IPropBag;

        IProvideACViewManager GetOrAddViewManagerProvider_New<TDal, TSource, TDestination>
        (
            IPropBag propBag,   // The client of this service.
            IViewManagerProviderKey viewManagerProviderKey,
            CrudWithMappingCreator<TDal, TSource, TDestination> crudWithMappingCreator,
            CViewProviderCreator viewBuilder            // Method that can be used to create a IProvideAView from a DataSourceProvider.
        )
        where TDal : class, IDoCRUD<TSource>
        where TSource : class
                    where TDestination : class, INotifyItemEndEdit, IPropBag;

        bool TryGetViewManagerProvider
        (
            IPropBag propBag,   // The client of this service.
            IViewManagerProviderKey viewManagerProviderKey,
            out IProvideACViewManager provideACViewManager
        );

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }

        //bool PBTestSet(PropBagAbstractBase x);
    }


}