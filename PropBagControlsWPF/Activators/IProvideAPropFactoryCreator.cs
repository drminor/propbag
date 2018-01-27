using DRM.TypeSafePropertyBag;

namespace DRM.PropBagControlsWPF
{
    public interface IProvideAPropFactoryCreator
    {
        IPropFactory GetNewPropFactory();
    }
}