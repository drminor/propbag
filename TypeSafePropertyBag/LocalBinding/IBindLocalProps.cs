
namespace DRM.TypeSafePropertyBag
{
    public interface IBindLocalProps<L2T>
    {
        //bool TryGetPropData(SimpleExKey propId, out IPropGen propData);
        void UpdateTarget<T>(BindingSubscription<T> bs, T oldValue, T newValue, ref int counter);
    }
}