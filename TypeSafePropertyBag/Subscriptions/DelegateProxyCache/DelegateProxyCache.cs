using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    public class DelegateProxyCache
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

    }
}
