using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.ControlModel
{
    public interface IPropModelProvider
    {
        //bool HasPbtLookupResources { get; }
        //bool CanFindPropBagTemplateWithJustKey { get; }

        PropModel GetPropModel(string resourceKey, IPropFactory propFactory = null);
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey);
        //PropModel GetPropModel(PropBagTemplate pbt);
    }
}
