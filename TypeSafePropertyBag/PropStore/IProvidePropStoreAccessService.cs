
using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropStoreAccessService<L2T, L2TRaw> : ICacheSubscriptions<L2T>, IDisposable
    {
        // Information necessary to create composite keys.
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // Create and TearDown PropStoreAccessService instances.
        // TODO: Consider not supporting this method: The client must keep the reference.
        IPropStoreAccessService<L2T, L2TRaw> GetOrCreatePropStoreService(IPropBag propBag, IL2KeyMan<L2T, L2TRaw> level2KeyManager);
        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag, IL2KeyMan<L2T, L2TRaw> level2KeyManager);
        void TearDown(IPropStoreAccessService<L2T, L2TRaw> propStoreAccessService);

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }

        int TotalNumberOfAccessServicesCreated { get; }
        int NumberOfRootPropBagsInPlay { get; }
    }
}