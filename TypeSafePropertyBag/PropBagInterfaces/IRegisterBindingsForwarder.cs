
using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IRegisterBindingsForwarder<L2T> 
    {
        bool RegisterBinding<T>(L2T propId, LocalBindingInfo bindingInfo);

        bool UnregisterBinding<T>(L2T propId, LocalBindingInfo bindingInfo);
    }

    public interface IRegisterBindingsProxy<L2T>
    {
        bool RegisterBinding(IRegisterBindingsForwarder<L2T> forwarder, L2T propId, LocalBindingInfo bindingInfo);

        bool UnregisterBinding(IRegisterBindingsForwarder<L2T> forwarder, L2T propId, LocalBindingInfo bindingInfo);
    }
}
