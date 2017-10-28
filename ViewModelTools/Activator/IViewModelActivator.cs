using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    public interface IViewModelActivator<T> where T : class, IPropBag
    {
        //bool HasPbtLookupResources { get; }
        //bool CanFindPropBagTemplateWithJustKey { get; }
        //T GetNewViewModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);
        //T GetNewViewModel(PropBagTemplate pbt, IPropFactory propFactory);

        T GetNewViewModel(string resourceKey, IPropFactory propFactory);
        T GetNewViewModel(PropModel propModel, IPropFactory propFactory);

        T GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory) where BT : class, IPropBag;
        T GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory) where BT : class, IPropBag;

        T GetNewViewModel(string resourceKey, IPropFactory propFactory, Type baseType);
        T GetNewViewModel(PropModel propModel, IPropFactory propFactory, Type baseType);

    }
}
