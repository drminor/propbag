using System;

namespace PropBagLib.Tests.SpecBasedVMTests
{
    public class ContextCleaner : IDisposable
    {
        #region Private Members

        private readonly Action _cleanup;

        #endregion

        #region Constructor

        public ContextCleaner(Action cleanup)
        {
            _cleanup = cleanup;
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
                    _cleanup?.Invoke();
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
