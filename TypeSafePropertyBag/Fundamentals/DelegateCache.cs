using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class DelegateCache<T> where T : class
    { 
        LockingConcurrentDictionary<Type, T> _cache;
        MethodInfo _theMethod;

        public DelegateCache(MethodInfo theMethod)
        {
            _theMethod = theMethod;
            _cache = new LockingConcurrentDictionary<Type, T>(MakeTheDelegate);
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
            T result = _cache.GetOrAdd(typeOfThisValue);
            return result;
        }

        internal T MakeTheDelegate(Type typeOfThisValue)
        {
            MethodInfo methInfoSetProp = _theMethod.MakeGenericMethod(typeOfThisValue);
            Delegate result = Delegate.CreateDelegate(typeof(T), null, methInfoSetProp);
            return result as T;
        }
    }
}
