using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBagControlsWPF
{
    public interface IPropBagTemplateProvider
    {
        bool CanFindPropBagTemplateWithJustAKey { get; }

        PropBagTemplate GetPropBagTemplate(string resourceKey);
        PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string resourceKey);
        Dictionary<string, PropBagTemplate> GetPropBagTemplates(ResourceDictionary resources);
    }

    public interface IMapperRequestProvider
    {
        bool CanFindMapperRequestWithJustAKey { get; }

        MapperRequestTemplate GetMapperRequest(string resourceKey);
        MapperRequestTemplate GetMapperRequest(ResourceDictionary resources, string resourceKey);

        Dictionary<string, MapperRequestTemplate> GetMapperRequests(ResourceDictionary resources);

    }
}