
namespace DRM.TypeSafePropertyBag
{
    internal interface IHaveTheKey<CompT, L1T, L2T>
    {
        IExplodedKey<CompT, L1T, L2T> GetTheKey(IPropBag propBag, L2T propId);
    }
}
