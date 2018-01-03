using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag
{
    public interface IProvideHandlerDispatchDelegateCaches
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