using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using BridgeType = HandlerDispatchGenericBridges;

    public class SimpleHandlerDispatchDelegateCacheProvider : IProvideHandlerDispatchDelegateCaches, IDisposable
    {
        #region Public Properties

        TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate> _callPcTypedEventSubsCache;
        public TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate> CallPcTypedEventSubsCache => _callPcTypedEventSubsCache;

        DelegateCache<CallPcObjEventSubscriberDelegate> _callPcObjEventSubsCache;
        public DelegateCache<CallPcObjEventSubscriberDelegate> CallPcObjEventSubsCache => _callPcObjEventSubsCache;

        DelegateCache<CallPcGenEventSubscriberDelegate> _callPcGenEventSubsCache;
        public DelegateCache<CallPcGenEventSubscriberDelegate> CallPcGenEventSubsCache => _callPcGenEventSubsCache;

        DelegateCache<CallPcStandardEventSubscriberDelegate> _callPcStEventSubsCache;
        public DelegateCache<CallPcStandardEventSubscriberDelegate> CallPcStEventSubsCache => _callPcStEventSubsCache;

        DelegateCache<CallPChangingEventSubscriberDelegate> _callPChangingEventSubsCache;
        public DelegateCache<CallPChangingEventSubscriberDelegate> CallPChangingEventSubsCache => _callPChangingEventSubsCache;

        DelegateCache<CallPSParentNodeChangedEventSubDelegate> _callPSParentNodeChangedEventSubsCache;
        public DelegateCache<CallPSParentNodeChangedEventSubDelegate> CallPSParentNodeChangedEventSubsCache => _callPSParentNodeChangedEventSubsCache;

        DelegateProxyCache _delegateProxyCache;
        public DelegateProxyCache DelegateProxyCache => _delegateProxyCache;

        #endregion

        #region Constructor

        public SimpleHandlerDispatchDelegateCacheProvider()
        {
            Type bridgeType = typeof(BridgeType);

            // PcTyped
            MethodInfo callPcTypedEventSubscriber_mi = bridgeType.GetMethod("CallPcTypedEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            _callPcTypedEventSubsCache = new TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate>(callPcTypedEventSubscriber_mi);

            // PcGen
            MethodInfo callPcGenEventSubscriber_mi = bridgeType.GetMethod("CallPcGenEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            _callPcGenEventSubsCache = new DelegateCache<CallPcGenEventSubscriberDelegate>(callPcGenEventSubscriber_mi);

            // PcObject
            MethodInfo callPcObjEventSubscriber_mi = bridgeType.GetMethod("CallPcObjectEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            _callPcObjEventSubsCache = new DelegateCache<CallPcObjEventSubscriberDelegate>(callPcObjEventSubscriber_mi);

            // PcStandard
            MethodInfo callPcStEventSubscriber_mi = bridgeType.GetMethod("CallPcStEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            _callPcStEventSubsCache = new DelegateCache<CallPcStandardEventSubscriberDelegate>(callPcStEventSubscriber_mi);

            // PcChanging
            MethodInfo callPChangingEventSubscriber_mi = bridgeType.GetMethod("CallPChangingEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            _callPChangingEventSubsCache = new DelegateCache<CallPChangingEventSubscriberDelegate>(callPChangingEventSubscriber_mi);

            // PropStoreNode Parent Changed
            MethodInfo callPSNodeParentChangedEventSubscriber_mi = bridgeType.GetMethod("CallPSNodeParentChangedEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            _callPSParentNodeChangedEventSubsCache = new DelegateCache<CallPSParentNodeChangedEventSubDelegate>(callPSNodeParentChangedEventSubscriber_mi);

            // Proxy -- Holds a copy of the original delegate but without the target.
            _delegateProxyCache = new DelegateProxyCache();
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    _callPcGenEventSubsCache.Dispose();
                    _callPChangingEventSubsCache.Dispose();
                    _callPcObjEventSubsCache.Dispose();
                    _callPcStEventSubsCache.Dispose();
                    _callPcTypedEventSubsCache.Dispose();
                    _callPSParentNodeChangedEventSubsCache.Dispose();

                    _delegateProxyCache.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}
