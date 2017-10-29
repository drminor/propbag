using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeWrapper
{
    public class WrapperTypeLocalCache : ICacheWrapperTypes
    {
        //IEmitWrapperType _emitterEngine;
        IModuleBuilderInfo _moduleBuilderInfo;

        LockingConcurrentDictionary<TypeDescription, Type> _emittedTypes;

        public WrapperTypeLocalCache(IEmitWrapperType emitterEngine, IModuleBuilderInfo moduleBuilderInfo)
        {
            //_emitterEngine = emitterEngine;
            _moduleBuilderInfo = moduleBuilderInfo;
            _emittedTypes = new LockingConcurrentDictionary<TypeDescription, Type>(emitterEngine.EmitWrapperType);
        }

        public Type GetOrAdd(TypeDescription td)
        {
            return _emittedTypes.GetOrAdd(td);
        }
    }
}
