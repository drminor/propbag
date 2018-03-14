using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    public class DelegateProxyCache : IDisposable
    { 
        LockingConcurrentDictionary<MethodSubscriptionKind, Delegate> _cache;

        public DelegateProxyCache()
        {
            _cache = new LockingConcurrentDictionary<MethodSubscriptionKind, Delegate>(MakeTheDelegate);
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public Delegate GetOrAdd(MethodSubscriptionKind tsk)
        {
            Delegate result = _cache.GetOrAdd(tsk);
            return result;
        }

        internal Delegate MakeTheDelegate(MethodSubscriptionKind tsk)
        {
            Delegate result = Delegate.CreateDelegate(tsk.DelegateType, null, tsk.Method);
            return result;
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                _cache.Clear();

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
