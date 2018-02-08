namespace DRM.TypeSafePropertyBag
{
    public interface IProvidePropTemplates
    {
        int Count { get; }

        IPropTemplate GetOrAdd(IPropTemplate propTemplate);
    }
}