using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PropModelType = IPropModel<String>;


    internal interface IProvidePropStoreAccessService<L2T, L2TRaw> : IPropStoreAccessServiceCreator<L2T, L2TRaw>, IDisposable
    {
        bool TryGetPropBagNode(IPropBag propBag, out BagNode propBagNode);
        //bool TryGetPropBagNode(WeakRefKey<IPropBag> propBag_wrKey, out StoreNodeBag propBagNode);

        bool TearDown(BagNode propBagNode);

        bool IsPropItemSetFixed(BagNode propBagNode);

        //object FixPropItemSet(BagNode propBagNode);
        //bool TryOpenPropItemSet(BagNode propBagNode, out object propItemSet_Handle);
        bool TryFixPropItemSet(BagNode propBagNode, WeakRefKey<PropModelType> propItemSetId);
        bool TryOpenPropItemSet(BagNode propBagNode/*, out object propItemSet_Handle*/);

        object GetValueFast(WeakRefKey<IPropBag> propBag_wrKey, WeakRefKey<PropModelType> propItemSetId, ExKeyT compKey);
        bool SetValueFast(WeakRefKey<IPropBag> propBag_wrKey, WeakRefKey<PropModelType> propItemSetId, ExKeyT compKey, object value);

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

        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag, IPropNodeCollection<L2T, L2TRaw> template);

        bool IsPropItemSetFixed(IPropBag propBag);

        //object FixPropItemSet(IPropBag propBag);
        //bool TryOpenPropItemSet(IPropBag propBag, out object propItemSet_Handle);

        bool TryFixPropItemSet(IPropBag propBag, WeakRefKey<PropModelType> propItemSetId);
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