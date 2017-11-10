using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public interface IBindLocalProps<PropDataT> where PropDataT : IPropGen
    {
        //bool TryGetPropData(SimpleExKey propId, out PropDataT propData);
        void UpdateTarget<T>(/*IPropBag sourceHost, */BindingSubscription<T> bs, T oldValue, T newValue, ref int counter);
    }
}