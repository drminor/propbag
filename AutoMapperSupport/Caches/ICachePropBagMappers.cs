namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICachePropBagMappers
    {
        void RegisterMapperRequest(IPropBagMapperKeyGen mapRequest);

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapRequest);
    }
}
