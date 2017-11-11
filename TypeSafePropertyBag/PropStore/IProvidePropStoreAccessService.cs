

using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropStoreAccessService<PropBagT, PropDataT>
        where PropBagT : IPropBag
        where PropDataT : IPropGen
    {
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // TODO: Consider not supporting this method: The client must keep the reference.
        IPropStoreAccessService<PropBagT, PropDataT> GetOrCreatePropStoreService(PropBagT propBag);

        IPropStoreAccessService<PropBagT, PropDataT> CreatePropStoreService(PropBagT propBag);
    }
}