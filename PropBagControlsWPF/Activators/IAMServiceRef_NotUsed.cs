using SWHP.AutoMapperSupport;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBagControlsWPF
{
    public interface IAMServiceRef
    {
        IProvideAutoMappers AutoMapperProvider { get; }
        SimpleExKey ExKey { get; }
    }
}