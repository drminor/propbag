namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICachePropBagMappers
    {
        IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapRequest);

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapRequest);
    }
}
