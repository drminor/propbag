using System;

namespace DRM.TypeWrapper
{
    public interface IEmitWrapperType
    {
        Type EmitWrapperType(TypeDescription td);
    }
}
