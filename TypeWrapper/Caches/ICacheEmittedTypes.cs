using System;

namespace DRM.PropBag.TypeWrapper
{
    public interface ICacheEmittedTypes
    {
        Type GetOrAdd(TypeDescription td);

        long ClearTypeCache();

    }
}
