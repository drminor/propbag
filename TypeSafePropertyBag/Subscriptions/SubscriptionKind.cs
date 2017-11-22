﻿
namespace DRM.TypeSafePropertyBag
{
    public enum SubscriptionKind
    {
        TypedHandler, //PCTypedEventArgs<T>
        GenHandler, //PCGenEventArgs

        StandardHandler, // PropertyChangedEventArgs

        TypedAction,
        ObjectAction,

        ActionNoParams, // Used by the IDisposable implementation.

        LocalBinding
    }
}