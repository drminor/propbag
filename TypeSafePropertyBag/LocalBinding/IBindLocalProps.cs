
namespace DRM.TypeSafePropertyBag.EventManagement
{
    public interface IBindLocalProps<L2Type>
    {
        //bool TryGetPropData(SimpleExKey propId, out PropDataT propData);
        void UpdateTarget<T>(BindingSubscription<T> bs, T oldValue, T newValue, ref int counter);
    }
}