using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    public interface IViewModelActivator
    {
        //bool HasPbtLookupResources { get; }
        //bool CanFindPropBagTemplateWithJustKey { get; }
        //object GetNewViewModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);
        //object GetNewViewModel(PropBagTemplate pbt, IPropFactory propFactory);

        // Create new Type that is derived from a Type known only at run time.
        object GetNewViewModel(string resourceKey, Type typeToCreate, IPropFactory propFactory = null);
        object GetNewViewModel(PropModel propModel, Type typeToCreate, IPropFactory propFactory = null);

        // Create new Type that is derived from a Type known at compile time.
        BT GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory = null) where BT : class, IPropBag;
        BT GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory = null) where BT : class, IPropBag;
    }

    //public interface IViewModelActivator_OLD<T> where T : class, IPropBag
    //{
    //    //bool HasPbtLookupResources { get; }
    //    //bool CanFindPropBagTemplateWithJustKey { get; }
    //    //T GetNewViewModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);
    //    //T GetNewViewModel(PropBagTemplate pbt, IPropFactory propFactory);

    //    T GetNewViewModel(string resourceKey, IPropFactory propFactory);
    //    T GetNewViewModel(PropModel propModel, IPropFactory propFactory);

    //    T GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory) where BT : class, IPropBag;
    //    T GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory) where BT : class, IPropBag;

    //    T GetNewViewModel(string resourceKey, IPropFactory propFactory, Type baseType);
    //    T GetNewViewModel(PropModel propModel, IPropFactory propFactory, Type baseType);

    //}
}
