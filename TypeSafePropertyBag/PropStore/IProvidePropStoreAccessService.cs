
using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public interface IProvidePropStoreAccessService<L2T, L2TRaw> : IDisposable
    {
        // Information necessary to create composite keys.
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // Create and TearDown PropStoreAccessService instances.
        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBagInternal propBag);

        bool TearDown(ExKeyT compKey);

        // Diagnostics 
        void IncAccess();
        int AccessCounter { get; }
        void ResetAccessCounter();

        int TotalNumberOfAccessServicesCreated { get; }
        int NumberOfRootPropBagsInPlay { get; }
    }

    internal interface IProvidePropStoreCloneService<L2T, L2TRaw>
    {
        IPropStoreAccessService<L2T, L2TRaw> CloneService(IPropBagInternal sourcePropBag, IPropStoreAccessService<L2T, L2TRaw> sourceAccessService, IPropBagInternal targetPropBag, out StoreNodeBag sourceStoreNode,  out StoreNodeBag newStoreNode);
    }
}