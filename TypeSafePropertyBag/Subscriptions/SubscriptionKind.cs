
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionKind : int
    {
        TypedHandler, //PCTypedEventArgs<T>
        GenHandler, //PCGenEventArgs, same as ObjHandler but also includes PropertyType.

        ObjHandler, //PCObjectEventArgs

        StandardHandler, // PropertyChangedEventArgs
        ChangingHandler, // PropertyChangingEventArgs

        TypedAction,
        ObjectAction,

        ActionNoParams, // Used by the IDisposable implementation.

        LocalBinding
    }
}
