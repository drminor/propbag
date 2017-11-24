
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionKind
    {
        TypedHandler, //PCTypedEventArgs<T>
        GenHandler, //PCGenEventArgs

        ObjHandler, //Same as GenHander, but no chance of being cast to a TypedHandler

        StandardHandler, // PropertyChangedEventArgs

        TypedAction,
        ObjectAction,

        ActionNoParams, // Used by the IDisposable implementation.

        LocalBinding
    }
}
