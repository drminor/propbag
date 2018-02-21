using System;

namespace DRM.TypeSafePropertyBag
{
    internal interface IProvidePropStoreAccessService<L2T, L2TRaw> : IPropStoreAccessServiceCreator<L2T, L2TRaw>, IDisposable
    {
        bool TryGetPropBagNode(IPropBag propBag, out BagNode propBagNode);
        //bool TryGetPropBagNode(WeakRefKey<IPropBag> propBag_wrKey, out StoreNodeBag propBagNode);

        bool TearDown(BagNode propBagNode);

        //object FixPropItemSet(BagNode propBagNode);
        object FixPropItemSet(IPropNodeCollection_Internal<L2T, L2TRaw> pnc_int);
        bool TryOpenPropItemSet(BagNode propBagNode, out object propItemSet_Handle);

        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag, IPropNodeCollection_Internal<L2T, L2TRaw> template, out BagNode newBagNode);

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

        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag, object propItemSetHandle);


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