using System;

namespace DRM.PropBag.ControlModel
{
    public interface IPropModelProvider
    {
        //bool HasPbtLookupResources { get; }
        //bool CanFindPropBagTemplateWithJustKey { get; }

        PropModel GetPropModel(string resourceKey);
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey);
        //PropModel GetPropModel(PropBagTemplate pbt);
    }
}
