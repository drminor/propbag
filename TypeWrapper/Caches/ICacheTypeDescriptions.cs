﻿
namespace DRM.PropBag.TypeWrapper
{
    public interface ICacheTypeDescriptions
    {
        TypeDescription GetOrAdd(NewTypeRequest request);
    }
}
