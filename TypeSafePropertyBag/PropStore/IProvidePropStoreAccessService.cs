using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropItemSetKeyType = PropItemSetKey<String>;


    internal interface IProvidePropStoreAccessService<L2T, L2TRaw> : IPropStoreAccessServiceCreator<L2T, L2TRaw>, IDisposable
    {
        bool TryGetPropBagNode(IPropBag propBag, out BagNode propBagNode);

        bool TearDown(BagNode propBagNode);

        bool IsPropItemSetFixed(BagNode propBagNode);

        bool TryFixPropItemSet(BagNode propBagNode, PropItemSetKeyType propItemSetKey);
        bool TryOpenPropItemSet(BagNode propBagNode/*, out object propItemSet_Handle*/);

        IPropStoreFastAccess<L2T, L2TRaw> GetFastAccessService();

        object GetValueFast(WeakRefKey<IPropBag> propBag_wrKey, L2T propId, PropItemSetKeyType propItemSetKey);
        bool SetValueFast(WeakRefKey<IPropBag> propBag_wrKey, L2T propId, PropItemSetKeyType propItemSetKey, object value);

        object GetValueFast(BagNode propBagNode, L2T propId, PropItemSetKeyType propItemSetKey);
        bool SetValueFast(BagNode propBagNode, L2T propId, PropItemSetKeyType propItemSetKey, object value);

        object GetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey);
        bool SetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey, object value);


        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService
        (
            IPropBag propBag,
            IPropNodeCollection_Internal<L2T, L2TRaw> template,
            out BagNode newBagNode
        );

        IPropStoreAccessService<L2T, L2TRaw> ClonePSAccessService
        (
            IPropNodeCollection_Internal<L2T, L2TRaw> template,
            IPropBag targetPropBag,
            out BagNode newStoreNode
        );

        IPropStoreAccessServiceCreator<L2T, L2TRaw> StoreAcessorCreator { get; }
    }

    public interface IPropStoreAccessServiceCreator<L2T, L2TRaw> : IPropStoreAccessServicePerf<L2T, L2TRaw>
    {
        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag);

        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag, IPropNodeCollection<L2T, L2TRaw> template);

        bool IsPropItemSetFixed(IPropBag propBag);

        bool TryFixPropItemSet(IPropBag propBag, PropItemSetKeyType propItemSetKey);
        bool TryOpenPropItemSet(IPropBag propBag/*, out object propItemSet_Handle*/);

        // Information necessary to create composite keys.
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }
    }

    public interface IPropStoreAccessServicePerf<L2T, L2TRaw>
    {
        // Diagnostics 
        void IncAccess();
        int AccessCounter { get; }
        void ResetAccessCounter();

        int TotalNumberOfAccessServicesCreated { get; }
        int NumberOfRootPropBagsInPlay { get; }
    }
}