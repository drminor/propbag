
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
        void TearDown(ExKeyT cKey);

        // Diagnostics 
        void IncAccess();
        int AccessCounter { get; }
        void ResetAccessCounter();

        int TotalNumberOfAccessServicesCreated { get; }
        int NumberOfRootPropBagsInPlay { get; }
    }

    internal interface IProvidePropStoreCloneService<L2T, L2TRaw>
    {
        IPropStoreAccessService<L2T, L2TRaw> CloneService(IPropStoreAccessService<L2T, L2TRaw> copySource, IPropBagInternal target, out StoreNodeBag newStoreNode);
    }
}