using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBag.ControlsWPF
{
    public interface IPropBagTemplateProvider
    {
        bool CanFindPropBagTemplateWithJustKey { get; }

        PropBagTemplate GetPropBagTemplate(string instanceKey);
        PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string instanceKey);
        Dictionary<string, PropBagTemplate> GetPropBagTemplates(ResourceDictionary resources);
    }
}