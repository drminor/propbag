using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBag.ControlsWPF
{
    public interface IPropBagTemplateProvider
    {
        bool CanFindPropBagTemplateWithJustKey { get; }

        PropBagTemplate GetPropBagTemplate(string resourceKey);
        PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string resourceKey);
        Dictionary<string, PropBagTemplate> GetPropBagTemplates(ResourceDictionary resources);
    }
}