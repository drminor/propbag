
namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Provides 'raw' access to the PropStore for classes internal to the TypeSafePropBag assembly (and friends)
    /// without requiring the classes that implement this interface to also be internal.
    /// </summary>
    internal interface IHaveTheStoreNode
    {
        //L1T ObjectId { get; }

        ///// <summary>
        ///// Produces a Key containing the ObjectId assigned to the PropBag that holds the Prop object
        ///// identifed by the PropData Id. Note: PropData Ids are unique only in the context of its PropBag object.
        ///// This produces a global key for the given PropData object.
        ///// </summary>
        ///// <param name="propBag"></param>
        ///// <param name="propId"></param>
        ///// <returns></returns>
        //IExplodedKey<CompT, L1T, L2T> GetTheKey(IPropBag propBag, L2T propId);

        PropStoreNode PropStoreNode { get; }

        //PropStoreNode GetObjectNodeForPropVal(IPropDataInternal int_propData);

        //bool TryGetAChildOfMine(L2T propId, out PropStoreNode propNode);
    }
}
