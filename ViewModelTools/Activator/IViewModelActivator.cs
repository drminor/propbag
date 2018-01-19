using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ViewModelTools
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public interface IViewModelActivator
    {
        bool HasPropModelLookupService { get; }

        // Create new Type that is derived from a Type known only at run time.
        //object GetNewViewModel(string resourceKey, Type typeToCreate, string fullClassName = null, IPropFactory propFactory = null);
        object GetNewViewModel(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, Type typeToCreate, IPropFactory propFactory = null, string fullClassName = null);

        // Create new Type that is derived from a Type known at compile time.
        //object GetNewViewModel<BT>(string resourceKey, string fullClassName = null, IPropFactory propFactory = null) where BT : class, IPropBag;
        object GetNewViewModel<BT>(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory = null, string fullClassName = null) where BT : class, IPropBag;

        object GetNewViewModel(Type typeToCreate, IPropBagInternal copySource);
        object GetNewViewModel<BT>(IPropBagInternal copySource) where BT : class, IPropBag;

    }
}
