using DRM.TypeSafePropertyBag.DataAccessSupport;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropStoreAccessService<L2T, L2TRaw> : IRegisterSubscriptions<L2T>, IRegisterBindings<L2T>, IDisposable
    {
        int PropertyCount { get; }

        bool TryGetPropId(L2TRaw propertyName, out L2T propId);
        bool TryGetPropName(L2T propertyId, out L2TRaw propertyName);
        L2T Add(L2TRaw propertyName);

        // IDictionary-Like Methods
        IPropData this[IPropBag propBag, L2T propId] { get; }

        bool ContainsKey(IPropBag propBag, L2T propId);

        IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropData>> GetEnumerator(IPropBag propBag);
        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropData> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, out IPropData propData);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, EventHandler<PcGenEventArgs> handler, SubscriptionPriorityGroup priorityGroup, out IPropData propData);
        bool TryAdd<PropT>(IPropBag propBag, L2T propId, IProp genericTypedProp, EventHandler<PcTypedEventArgs<PropT>> handler, SubscriptionPriorityGroup priorityGroup, out IPropData propData);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out IPropData propData);

        bool TryGetValue(IPropBag propBag, L2T propId, out IPropData propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropData propData);

        // Restricted Update Method on Dictionary-like object.
        bool SetTypedProp(IPropBag propBag, L2T propId, L2TRaw propertyName, IProp genericTypedProp);

        IPropStoreAccessService<L2T, L2TRaw> CloneProps(IPropBag callingPropBag, IPropBagInternal copySource);
        int ClearAllProps(IPropBag propBag);

        // Collection View Related
        //IProvideADataSourceProvider GetDataSourceProviderProvider(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder);

        IManageCViews GetViewManager(IPropBag propBag, L2T propId);

        DataSourceProvider GetDataSourceProvider(IPropBag propBag, L2T propId);

        DataSourceProvider GetOrAddDataSourceProvider(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder);

        IManageCViews GetOrAddViewManager(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder);
        IManageCViews GetOrAddViewManager(IPropBag propBag, L2T propId, IPropData propData, CViewProviderCreator viewBuilder, IProvideADataSourceProvider dSProviderProvider);

        // Providing a Mapper directly
        IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
            (
            IPropBag propBag, // The client to this PropStore Access Service
            L2T propId, // Identifies the PropItem that implements IDoCrud<TSource>
            IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
            IPropBagMapper<TSource, TDestination> mapper,
            CViewProviderCreator viewBuilder // Method that can be used to create a IProvideAView from a DataSourceProvider.
            )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit;

        // Using a IMapperRequest and Factory.
        IManageCViews GetOrAddViewManager<TDal, TSource, TDestination>
            (
            IPropBag propBag, // The client to this PropStore Access Service
            L2T propId, // Identifies the PropItem that implements IDoCrud<TSource>
            IPropData propData, // The PropStore management wrapper for IProp<TSource> which holds the value of the 'IDoCrud<T>' data access layer.
            IMapperRequest mr, // The information necessar to create a IPropBagMapper<TSource, TDestination>
            PropBagMapperCreator propBagMapperCreator, // A delegate that can be called to create a IPropBagMapper<TSource, TDestination> given a IMapperRequest.
            CViewProviderCreator viewBuilder // Method that can be used to create a IProvideAView from a DataSourceProvider.
            )
            where TDal : class, IDoCRUD<TSource>
            where TSource : class
            where TDestination : INotifyItemEndEdit;

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }
    }
}