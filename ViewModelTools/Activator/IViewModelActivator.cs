using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    public interface IViewModelActivator
    {
        bool HasPropModelLookupService { get; }

        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(string resourceKey, Type typeToCreate, string fullClassName = null, IPropFactory propFactory = null);
        object GetNewViewModel(PropModel propModel, Type typeToCreate, string fullClassName = null, IPropFactory propFactory = null);

        // Create new Type that is derived from a Type known at compile time.
        object GetNewViewModel<BT>(string resourceKey, string fullClassName = null, IPropFactory propFactory = null) where BT : class, IPropBag;
        object GetNewViewModel<BT>(PropModel propModel, string fullClassName = null, IPropFactory propFactory = null) where BT : class, IPropBag;

        object GetNewViewModel(Type typeToCreate, IPropBag copySource);
        object GetNewViewModel<BT>(IPropBag copySource) where BT : class, IPropBag;

    }
}
