namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public interface IExplodedKey<CompT, L1T, L2T>
    {
        CompT CKey { get; }
        L1T Level1Key { get; }
        L2T Level2Key { get; }
    }
}
