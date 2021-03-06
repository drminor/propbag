﻿
using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IRegisterBindings<L2T> : ICacheBindings<L2T>
    {
        bool RegisterBinding<T>(IPropBag targetPropBag, L2T propId, LocalBindingInfo bindingInfo);

        bool UnregisterBinding<T>(IPropBag targetPropBag, L2T propId, LocalBindingInfo bindingInfo);
    }

}
