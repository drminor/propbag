
using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IRegisterBindings<L2T> : ICacheBindings<L2T>
    {
        bool RegisterBinding<T>(IPropBag targetPropBag, L2T propId, LocalBindingInfo bindingInfo);

        bool UnRegisterBinding<T>(IPropBag targetPropBag, L2T propId, LocalBindingInfo bindingInfo);

        //bool RegisterBinding(IPropBag targetPropBag, L2T propId, LocalBindingInfo bindingInfo);

        //bool UnRegisterBinding(IPropBag targetPropBag, L2T propId, LocalBindingInfo bindingInfo);
    }
}
