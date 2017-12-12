using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public class DelegateCache<T> where T : class
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

        internal T MakeTheDelegate(TypeKey tKey)
        {
            MethodInfo methInfoSetProp = _theMethod.MakeGenericMethod(tKey.Type);
            Delegate result = Delegate.CreateDelegate(typeof(T), null, methInfoSetProp);
            return result as T;
        }

    }
}
