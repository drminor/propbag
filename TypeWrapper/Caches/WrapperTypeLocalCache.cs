using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.PropBag.TypeWrapper
{
    public class WrapperTypeLocalCache : ICacheWrapperTypes
    {
        LockingConcurrentDictionary<TypeDescription, Type> _emittedTypes;

        public WrapperTypeLocalCache(IEmitWrapperType emitterEngine)
        {
            _emittedTypes = new LockingConcurrentDictionary<TypeDescription, Type>(emitterEngine.EmitWrapperType);
        }

        public Type GetOrAdd(TypeDescription td)
        {
            return _emittedTypes.GetOrAdd(td);
        }

        public long ClearTypeCache()
        {
            long result = _emittedTypes.Count;
            _emittedTypes.Clear();
            return result;
        }

    }
}
