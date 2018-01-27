using System;

namespace DRM.TypeSafePropertyBag
{
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    internal class Unsubscriber : IDisposable
    {
        private WeakReference<PSAccessServiceInterface> _propStoreAccessService_Wr { get; }
        ISubscriptionKeyGen _request;

        public Unsubscriber(WeakReference<PSAccessServiceInterface> wr_us, ISubscriptionKeyGen request)
        {
            _propStoreAccessService_Wr = wr_us;
            _request = request;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_propStoreAccessService_Wr.TryGetTarget(out PSAccessServiceInterface accService))
                    {
                        accService.RemoveSubscription(_request);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

        }

        #endregion
    }

}
