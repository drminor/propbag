using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Reflection;

namespace DRM.PropBag.Caches.NotUsed
{
    internal class DoSetDelegateCache
    { 
        LockingConcurrentDictionary<Type, DoSetDelegate> _cache;
        MethodInfo _doSetMethodInfo;

        public DoSetDelegateCache(Type hostType)
        {
            if(!hostType.IsPropBagBased())
            {
                throw new ArgumentException("The hostType must implement IPropBag.", nameof(hostType));
            }

            _doSetMethodInfo = hostType.GetMethod("DoSetBridge", BindingFlags.Instance | BindingFlags.NonPublic);
            _cache = new LockingConcurrentDictionary<Type, DoSetDelegate>(GetDoSetDelegate);
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public DoSetDelegate GetOrAdd(Type typeOfThisValue)
        {
            DoSetDelegate result = _cache.GetOrAdd(typeOfThisValue);
            return result;
        }

        internal DoSetDelegate GetDoSetDelegate(Type typeOfThisValue)
        {
            MethodInfo methInfoSetProp = _doSetMethodInfo.MakeGenericMethod(typeOfThisValue);
            DoSetDelegate result = (DoSetDelegate)Delegate.CreateDelegate(typeof(DoSetDelegate), null, methInfoSetProp);

            return result;
        }


    }
}
