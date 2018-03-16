using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBagControlsWPF
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public interface IParsePropBagTemplates
    {
        PropModelType ParsePropModel(IPropBagTemplate pbt);
    }
}