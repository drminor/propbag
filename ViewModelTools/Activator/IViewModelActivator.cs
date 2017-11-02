using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    public interface IViewModelActivator
    {
        //bool HasTypeCreationService { get; }

        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(string resourceKey, Type typeToCreate, IPropFactory propFactory = null);
        object GetNewViewModel(PropModel propModel, Type typeToCreate, IPropFactory propFactory = null);

        // Create new Type that is derived from a Type known at compile time.
        object GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory = null) where BT : class, IPropBag;
        object GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory = null) where BT : class, IPropBag;
    }
}
