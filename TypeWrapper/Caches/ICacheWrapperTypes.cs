using System;

namespace DRM.TypeWrapper
{
    public interface ICacheWrapperTypes
    {
        Type GetOrAdd(TypeDescription td);
    }
}
