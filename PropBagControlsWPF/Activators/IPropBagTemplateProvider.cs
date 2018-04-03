using DRM.TypeSafePropertyBag;
using System.Collections.Generic;
using System.Windows;

namespace DRM.PropBagControlsWPF
{
    public interface IPropBagTemplateBuilder
    {
        bool CanFindPropBagTemplateWithJustAKey { get; }

        PropBagTemplate GetPropBagTemplate(string resourceKey);
        PropBagTemplate GetPropBagTemplate(ResourceDictionary resources, string resourceKey);

        IDictionary<string, IPropBagTemplate> GetPropBagTemplates();
        IDictionary<string, IPropBagTemplate> GetPropBagTemplates(ResourceDictionary resources);
    }

    public interface IMapperRequestBuilder
    {
        bool CanFindMapperRequestWithJustAKey { get; }

        MapperRequestTemplate GetMapperRequest(string resourceKey);
        MapperRequestTemplate GetMapperRequest(ResourceDictionary resources, string resourceKey);

        Dictionary<string, MapperRequestTemplate> GetMapperRequests(ResourceDictionary resources);
    }
}