
namespace DRM.TypeSafePropertyBag
{
    public delegate bool DoSetDelegate(IPropBag target, uint propId, string propertyName, IProp prop, object value);

    public delegate IProp CVPropFromDsDelegate(IPropBag target, string propertyName, string srcPropName, IMapperRequest mr);
}
