using DRM.PropBag.AutoMapperSupport;

namespace DRM.PropBagControlsWPF
{
    public interface IAMServiceRef
    {
        IProvideAutoMappers AutoMapperProvider { get; } 
    }
}