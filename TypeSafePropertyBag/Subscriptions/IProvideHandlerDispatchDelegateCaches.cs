using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideHandlerDispatchDelegateCaches : IDisposable
    {
        DelegateCache<CallPcGenEventSubscriberDelegate> CallPcGenEventSubsCache { get; }
        DelegateCache<CallPChangingEventSubscriberDelegate> CallPChangingEventSubsCache { get; }
        DelegateCache<CallPcObjEventSubscriberDelegate> CallPcObjEventSubsCache { get; }
        DelegateCache<CallPcStandardEventSubscriberDelegate> CallPcStEventSubsCache { get; }

        TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate> CallPcTypedEventSubsCache { get; }

        DelegateCache<CallPSParentNodeChangedEventSubDelegate> CallPSParentNodeChangedEventSubsCache { get; }


        DelegateProxyCache DelegateProxyCache { get; }
    }
}