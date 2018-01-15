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

        MapperRequest GetMapperRequest(string resourceKey);
        MapperRequest GetMapperRequest(ResourceDictionary resources, string resourceKey);

        Dictionary<string, MapperRequest> GetMapperRequests(ResourceDictionary resources);

    }
}