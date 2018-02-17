namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropModels
    {
        IPropModel GetPropModel(string resourceKey);

        IMapperRequest GetMapperRequest(string resourceKey);

        // In order to include these methods, a reference to PresentationFramework must be added.
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey);
        //PropModel GetPropModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory);
    }
}
