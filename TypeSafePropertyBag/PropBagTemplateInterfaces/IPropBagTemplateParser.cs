using System;

namespace DRM.TypeSafePropertyBag
{
    using PropModelType = IPropModel<String>;

    public interface IParsePropBagTemplates
    {
        PropModelType ParsePropModel(IPropBagTemplate pbt);
    }
}