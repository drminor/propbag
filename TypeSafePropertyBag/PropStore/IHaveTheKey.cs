
namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Allows access from code in the TypeSafePropertyBag assembly, but not from the PropBag assembly.
    /// </summary>
    internal interface IHaveTheKey<CompT, L1T, L2T>
    {
        L1T ObjectId { get; }

        /// <summary>
        /// Produces a Key containing the ObjectId assigned to the PropBag that holds the Prop object
        /// identifed by the PropData Id. Note: PropData Ids are unique only in the context of its PropBag object.
        /// This produces a global key for the given PropData object.
        /// </summary>
        /// <param name="propBag"></param>
        /// <param name="propId"></param>
        /// <returns></returns>
        IExplodedKey<CompT, L1T, L2T> GetTheKey(IPropBag propBag, L2T propId);

        // Returns the Globally Unique identifer for the Parent PropData item
        // if this PropStoreAccessService is serving a PropBag that is held within another IPropBag's PropData item.
        //IExplodedKey<CompT, L1T, L2T> ParentKey { get; set; }
    }
}
