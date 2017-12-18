using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using BridgeType = HandlerDispatchGenericBridges;

    public class SimpleHandlerDispatchDelegateCacheProvider : IProvideHandlerDispatchDelegateCaches
    {
        #region Public Properties

        public TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate> CallPcTypedEventSubsCache { get; }

        public DelegateCache<CallPcObjEventSubscriberDelegate> CallPcObjEventSubsCache { get; }

        public DelegateCache<CallPcGenEventSubscriberDelegate> CallPcGenEventSubsCache { get; }

        public DelegateCache<CallPcStandardEventSubscriberDelegate> CallPcStEventSubsCache { get; }

        public DelegateCache<CallPChangingEventSubscriberDelegate> CallPChangingEventSubsCache { get; }

        public DelegateProxyCache DelegateProxyCache { get; }

        #endregion

        public SimpleHandlerDispatchDelegateCacheProvider()
        {
            Type bridgeType = typeof(BridgeType);

            // PcTyped
            MethodInfo callPcTypedEventSubscriber_mi = bridgeType.GetMethod("CallPcTypedEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            CallPcTypedEventSubsCache = new TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate>(callPcTypedEventSubscriber_mi);

            // PcGen
            MethodInfo callPcGenEventSubscriber_mi = bridgeType.GetMethod("CallPcGenEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            CallPcGenEventSubsCache = new DelegateCache<CallPcGenEventSubscriberDelegate>(callPcGenEventSubscriber_mi);

            // PcObject
            MethodInfo callPcObjEventSubscriber_mi = bridgeType.GetMethod("CallPcObjectEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            CallPcObjEventSubsCache = new DelegateCache<CallPcObjEventSubscriberDelegate>(callPcObjEventSubscriber_mi);

            // PcStandard
            MethodInfo callPcStEventSubscriber_mi = bridgeType.GetMethod("CallPcStEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            CallPcStEventSubsCache = new DelegateCache<CallPcStandardEventSubscriberDelegate>(callPcStEventSubscriber_mi);

            // PcChanging
            MethodInfo callPChangingEventSubscriber_mi = bridgeType.GetMethod("CallPChangingEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            CallPChangingEventSubsCache = new DelegateCache<CallPChangingEventSubscriberDelegate>(callPChangingEventSubscriber_mi);

            // Proxy -- Holds a copy of the original delegate but without the target.
            DelegateProxyCache = new DelegateProxyCache();
        }
    }
}
