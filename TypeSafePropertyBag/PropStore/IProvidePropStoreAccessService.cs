
namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropStoreAccessService<L2T, L2TRaw>
    {
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // TODO: Consider not supporting this method: The client must keep the reference.
        IPropStoreAccessService<L2T, L2TRaw> GetOrCreatePropStoreService(IPropBag propBag);

        IPropStoreAccessService<L2T, L2TRaw> CreatePropStoreService(IPropBag propBag);
    }
}