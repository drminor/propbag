using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeWrapper.TypeDesc
{
    public class TypeDescriptionLocalCache : ICacheTypeDescriptions
    {
        //ITypeDescriptionProvider _typeDescriptionProvider;

        LockingConcurrentDictionary<NewTypeRequest, TypeDescription> _typeDescriptions;

        public TypeDescriptionLocalCache(ITypeDescriptionProvider typeDescriptionProvider)
        {
            //_typeDescriptionProvider = typeDescriptionProvider;
            _typeDescriptions = new LockingConcurrentDictionary<NewTypeRequest, TypeDescription>
                (typeDescriptionProvider.GetTypeDescription);
        }

        public TypeDescription GetOrAdd(NewTypeRequest request)
        {
            return _typeDescriptions.GetOrAdd(request);
        }
    }
}
