
using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropStoreAccessService<L2T, L2TRaw> : ICacheSubscriptions<L2T>, IDisposable
    {
        // Information necessary to create composite keys.
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // Create and TearDown PropStoreAccessService instances.
        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag);
        void TearDown(IPropBag int_PropBag, IPropStoreAccessService<L2T, L2TRaw> propStoreService);

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }

        int TotalNumberOfAccessServicesCreated { get; }
        int NumberOfRootPropBagsInPlay { get; }
    }
}