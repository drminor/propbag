using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public interface IBindLocalProps<PropDataT> where PropDataT : IPropGen
    {
        PropDataT GetPropData(SimpleExKey propId);
        int UpdateTarget<T>(SimpleExKey targetPropId, T oldValue, T newValue);
    }
}