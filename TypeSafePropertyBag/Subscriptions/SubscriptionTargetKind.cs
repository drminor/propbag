
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionTargetKind
    {
        Standard,
        StandardKeepRef,
        PropBag,
        LocalWeakRef // WeakReference<IPropBag>
    }
}
