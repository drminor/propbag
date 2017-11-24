
namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    public enum ObservableSourceStatusEnum
    {
        NoType = 0,
        HasType,
        Ready,
        IsWatchingProp,
        IsWatchingColl,
        IsWatchingPropAndColl,
        Undetermined // This is used for DataSourceProviders
    }

}
