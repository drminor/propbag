using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBag.ControlsWPF.WPFHelpers
{
    public interface IPropBagTemplateProvider
    {
        bool HasPbtLookupResources { get; }
        bool CanFindPropBagTemplateWithJustKey { get; }

        PropBagTemplate GetPropBagTemplate(string instanceKey);
        PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string instanceKey);
        Dictionary<string, PropBagTemplate> GetPropBagTemplates(ResourceDictionary resources);
    }

    public interface IPropModelProvider
    {
        bool HasPbtLookupResources { get; }
        bool CanFindPropBagTemplateWithJustKey { get; }

        PropModel GetPropModel(string resourceKey);
        PropModel GetPropModel(ResourceDictionary rd, string resourceKey);
        PropModel GetPropModel(PropBagTemplate pbt);
    }

    public interface IViewModelActivator<T> where T : class, IPropBag
    {
        bool HasPbtLookupResources { get; }
        bool CanFindPropBagTemplateWithJustKey { get; }

        T GetNewViewModel(string resourceKey, IPropFactory propFactory);

        T GetNewViewModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);

        T GetNewViewModel(PropBagTemplate pbt, IPropFactory propFactory);

        T GetNewViewModel(PropModel propModel, IPropFactory propFactory);

    }
}