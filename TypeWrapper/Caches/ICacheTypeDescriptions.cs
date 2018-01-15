
namespace DRM.TypeWrapper
{
    public interface ICacheTypeDescriptions
    {
        TypeDescription GetOrAdd(NewTypeRequest request);
    }
}
