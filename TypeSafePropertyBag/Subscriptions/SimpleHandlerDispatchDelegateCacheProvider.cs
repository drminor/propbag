using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using BridgeType = HandlerDispatchGenericBridges;

    public class SimpleHandlerDispatchDelegateCacheProvider : IProvideHandlerDispatchDelegateCaches, IDisposable
    {
        #region Public Properties

        public TwoTypesDelegateCache<CallPcTypedEventSubscriberDelegate> CallPcTypedEventSubsCache { get; }

        public DelegateCache<CallPcObjEventSubscriberDelegate> CallPcObjEventSubsCache { get; }

        public DelegateCache<CallPcGenEventSubscriberDelegate> CallPcGenEventSubsCache { get; }

        public DelegateCache<CallPcStandardEventSubscriberDelegate> CallPcStEventSubsCache { get; }

        public DelegateCache<CallPChangingEventSubscriberDelegate> CallPChangingEventSubsCache { get; }

        public DelegateCache<CallPSParentNodeChangedEventSubDelegate> CallPSParentNodeChangedEventSubsCache { get; }


        public DelegateProxyCache DelegateProxyCache { get; }

        #endregion

        #region Constructor

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

            // PropStoreNode Parent Changed
            MethodInfo callPSNodeParentChangedEventSubscriber_mi = bridgeType.GetMethod("CallPSNodeParentChangedEventSubscriber", BindingFlags.Instance | BindingFlags.NonPublic);
            CallPSParentNodeChangedEventSubsCache = new DelegateCache<CallPSParentNodeChangedEventSubDelegate>(callPSNodeParentChangedEventSubscriber_mi);

            // Proxy -- Holds a copy of the original delegate but without the target.
            DelegateProxyCache = new DelegateProxyCache();
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
                    CallPcGenEventSubsCache.Dispose();
                    CallPChangingEventSubsCache.Dispose();
                    CallPcObjEventSubsCache.Dispose();
                    CallPcStEventSubsCache.Dispose();
                    CallPcTypedEventSubsCache.Dispose();
                    CallPSParentNodeChangedEventSubsCache.Dispose();

                    // TODO: Dispose of the DelegateProxyCache

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
