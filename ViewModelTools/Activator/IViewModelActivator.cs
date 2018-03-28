using DRM.TypeSafePropertyBag;

using System;

namespace DRM.PropBag.ViewModelTools
{
    public interface IViewModelActivator<L2T, L2TRaw>
    {
        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(Type typeToCreate, IPropModel<L2TRaw> propModel, IViewModelFactory<L2T, L2TRaw> viewModelFactory,
            object ams, IPropFactory pfOverride, string fcnOverride);

        // Create new Type that is derived from a Type known at compile time.
        object GetNewViewModel<BT>(IPropModel<L2TRaw> propModel, IViewModelFactory<L2T, L2TRaw> viewModelFactory,
            object ams, IPropFactory pfOverride, string fcnOverride) where BT : class, IPropBag;

        object GetNewViewModel(Type typeToCreate, IPropBag copySource);
        object GetNewViewModel<BT>(IPropBag copySource) where BT : class, IPropBag;
    }
}
