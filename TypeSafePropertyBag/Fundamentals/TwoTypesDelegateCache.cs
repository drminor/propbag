using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class TwoTypesDelegateCache<T> : ICacheDelegatesForTypePair<T> where T : class
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

    }
}
