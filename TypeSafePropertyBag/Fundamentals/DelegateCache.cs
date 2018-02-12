using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class DelegateCache<T> : ICacheDelegates<T>, IDisposable where T : class
    { 
        LockingConcurrentDictionary<TypeKey, T> _cache;
        MethodInfo _theMethod;
        Type[] _typeArguments;

        public DelegateCache(MethodInfo theMethod)
        {
            _cache = new LockingConcurrentDictionary<TypeKey, T>(MakeTheDelegate);
            _theMethod = theMethod;

            _typeArguments = _theMethod.GetGenericArguments();
            _typeArguments[0] = typeof(T);
            for(int ptr = 1; ptr < _typeArguments.Length; ptr++)
            {
                _typeArguments[ptr] = typeof(object);
            }
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public T GetOrAdd(Type typeOfThisValue)
        {
            T result = _cache.GetOrAdd(new TypeKey(typeOfThisValue));
            return result;
        }

        private T MakeTheDelegate(TypeKey tKey)
        {
            System.Diagnostics.Debug.WriteLine($"Creating new delegate of type: {typeof(T)} for {tKey.Type}.");

            MethodInfo methInfoSetProp = _theMethod.MakeGenericMethod(tKey.Type);
            Delegate result = Delegate.CreateDelegate(typeof(T), null, methInfoSetProp);
            return result as T;
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
