using System;

namespace DRM.TypeSafePropertyBag
{
    internal interface IProvidePropStoreAccessService<L2T, L2TRaw> : IPropStoreAccessServiceCreator<L2T, L2TRaw>, IDisposable
    {
        bool TryGetPropBagNode(IPropBag propBag, out StoreNodeBag propBagNode);
        //bool TryGetPropBagNode(WeakRefKey<IPropBag> propBag_wrKey, out StoreNodeBag propBagNode);

        bool TearDown(StoreNodeBag propBagNode);

        object FixPropItemSet(StoreNodeBag propBagNode);
        bool TryOpenPropItemSet(StoreNodeBag propBagNode, out object propItemSet_Handle);

        IPropStoreAccessService<L2T, L2TRaw> ClonePSAccessService
            (
            IPropBag sourcePropBag,
            //IPropStoreAccessService<L2T, L2TRaw> sourceAccessService,
            StoreNodeBag propBagNode,
            IPropBag targetPropBag,
            out StoreNodeBag newStoreNode
            );
    }

    public interface IPropStoreAccessServiceCreator<L2T, L2TRaw> : IPropStoreAccessServicePerf<L2T, L2TRaw>
    {
        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag);

        object FixPropItemSet(IPropBag propBag);
        bool TryOpenPropItemSet(IPropBag propBag, out object propItemSet_Handle);

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