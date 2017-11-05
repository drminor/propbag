using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICreateMapperRequests
    {
        IPropBagMapperKeyGen CreateMapperRequest(string resourceKey);
    }
}