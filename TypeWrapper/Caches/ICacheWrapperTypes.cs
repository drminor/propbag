using System;

namespace DRM.PropBag.TypeWrapper
{
    public interface ICacheWrapperTypes
    {
        Type GetOrAdd(TypeDescription td);
    }
}
