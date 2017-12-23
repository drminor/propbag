
namespace DRM.TypeSafePropertyBag
{
    internal interface IPropStoreAccessServiceInternal<L2T, L2TRaw> 
    {
        IL2KeyMan<L2T, L2TRaw> Level2KeyManager { get; }

        bool TryGetChildPropNode(StoreNodeProp sourcePropNode, L2TRaw propertyName, out StoreNodeProp child);
    }
}