
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionTargetKind
    {
        Standard,
        StandardKeepRef,
        PropBag,
        GlobalPropId // I.e., IExplodedKey<CompT, L1T, L2T>
    }
}
