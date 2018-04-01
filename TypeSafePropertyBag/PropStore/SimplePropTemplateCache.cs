using System;

namespace DRM.TypeSafePropertyBag
{
    public class SimplePropTemplateCache : IProvidePropTemplates, IDisposable
    {
        LockingConcurrentDictionary<IPropTemplate, IPropTemplate> _cache;

        object _sync = new object();

        public SimplePropTemplateCache()
        {
            _cache = new LockingConcurrentDictionary<IPropTemplate, IPropTemplate>(Factory, new PropTemplateGenComparer());
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public IPropTemplate GetOrAdd(IPropTemplate propTemplate)
        {
            ReportCacheHit(propTemplate);

            IPropTemplate result = _cache.GetOrAdd(propTemplate);
            return result;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void ReportCacheHit(IPropTemplate propTemplate)
        {
            lock (_sync)
            {
                if (!_cache.ContainsKey(propTemplate))
                {
                    System.Diagnostics.Debug.WriteLine($"Adding a new PropTemplate for type = {propTemplate.Type}; kind = {propTemplate.PropKind}.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Using existing PropTemplate for type = {propTemplate.Type}; kind = {propTemplate.PropKind}.");
                }
            }
        }

        private IPropTemplate Factory(IPropTemplate propTemplate)
        {
            return propTemplate;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    _cache.Clear();
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
