using System;

namespace DRM.PropBag.TypeWrapper
{
    public interface IEmitWrapperType
    {
        Type EmitWrapperType(TypeDescription td);
    }
}
