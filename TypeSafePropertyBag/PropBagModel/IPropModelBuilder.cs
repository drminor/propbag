using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using PropModelType = IPropModel<String>;

    public interface IPropModelBuilder
    {
        PropModelType GetPropModel(string resourceKey);

        IMapperRequest GetMapperRequest(string resourceKey);

        IDictionary<string, string> GetTypeToKeyMap();

        // In order to include these methods, a reference to PresentationFramework must be added.
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey);
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);
    }
}
