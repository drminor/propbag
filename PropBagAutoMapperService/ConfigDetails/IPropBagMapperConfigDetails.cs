using DRM.TypeSafePropertyBag;
using Swhp.AutoMapperSupport;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    public interface IPropBagMapperConfigDetails : IAutoMapperConfigDetails
    {
        IPropModel<string> PropModel { get; }
    }
}