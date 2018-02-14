using DRM.TypeSafePropertyBag;

namespace DRM.PropBagControlsWPF
{
    public interface IParsePropBagTemplates
    {
        IPropModel ParsePropModel(PropBagTemplate pbt);
    }
}