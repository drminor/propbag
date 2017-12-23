using System;

namespace DRM.TypeSafePropertyBag
{
    internal class Unsubscriber : IDisposable
    {
        WeakReference<SimplePropStoreAccessService> _wr;
        ISubscriptionKeyGen _request;

        public Unsubscriber(WeakReference<SimplePropStoreAccessService> wr_us, ISubscriptionKeyGen request)
        {
            _wr = wr_us;
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
                    if (_wr.TryGetTarget(out SimplePropStoreAccessService accService))
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
