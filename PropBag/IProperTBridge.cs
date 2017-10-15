namespace DRM.PropBag
{
    public interface IProperTBridge
    {
        object GetValue(IPropBagMin host, string propertyName);
        void SetValue(IPropBagMin host, string propertyName, object value);
    }
}