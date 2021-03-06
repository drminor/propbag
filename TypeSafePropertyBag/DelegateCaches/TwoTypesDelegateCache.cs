﻿using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.DelegateCaches
{
    public class TwoTypesDelegateCache<T> : ICacheDelegatesForTypePair<T>, IDisposable where T : class
    { 
        LockingConcurrentDictionary<TypePair, T> _cache;
        MethodInfo _theMethod;

        public TwoTypesDelegateCache(MethodInfo theMethod)
        {
            _cache = new LockingConcurrentDictionary<TypePair, T>(MakeTheDelegate);
            _theMethod = theMethod;
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public T GetOrAdd(TypePair argumentTypes)
        {
            T result = _cache.GetOrAdd(argumentTypes);
            return result;
        }

        private T MakeTheDelegate(TypePair argumentTypes)
        {
            System.Diagnostics.Debug.WriteLine($"Creating new delegate of type: {typeof(T)} for {argumentTypes.SourceType}/{argumentTypes.DestinationType}.");

            MethodInfo methInfoSetProp = _theMethod.MakeGenericMethod(argumentTypes.TypeArguments);
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
