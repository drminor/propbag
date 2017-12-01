using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ControlModel
{
    public interface IPropModelProvider
    {
        PropModel GetPropModel(string resourceKey);
        PropModel GetPropModel(string resourceKey, IPropFactory propFactory);

        // These would require adding a reference to PresentationFramework.
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey);
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);
    }
}
