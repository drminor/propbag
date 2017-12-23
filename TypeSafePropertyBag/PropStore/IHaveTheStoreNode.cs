
namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Provides 'raw' access to the PropStore for classes internal to the TypeSafePropBag assembly (and friends)
    /// without requiring the classes that implement this interface to also be internal.
    /// </summary>
    internal interface IHaveTheStoreNode
    {
        StoreNodeBag PropStoreNode { get; }

        //PropStoreNode GetObjectNodeForPropVal(IPropDataInternal int_propData);

        //bool TryGetAChildOfMine(L2T propId, out PropStoreNode propNode);

        //bool RegisterBinding<PropT>(IExplodedKey<CompT, L1T, L2T> explodedKey, LocalBindingInfo bindingInfo);
        //bool UnregisterBinding<PropT>(IExplodedKey<CompT, L1T, L2T> explodedKey, LocalBindingInfo bindingInfo);
    }
}
