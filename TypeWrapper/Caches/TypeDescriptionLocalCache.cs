using DRM.TypeSafePropertyBag.DelegateCaches;

namespace DRM.PropBag.TypeWrapper.TypeDesc
{
    public class TypeDescriptionLocalCache : ICacheTypeDescriptions
    {
        LockingConcurrentDictionary<NewTypeRequest, TypeDescription> _typeDescriptions;

        public TypeDescriptionLocalCache(ITypeDescriptionProvider typeDescriptionProvider)
        {
            _typeDescriptions = new LockingConcurrentDictionary<NewTypeRequest, TypeDescription>
                (typeDescriptionProvider.GetTypeDescription);
        }

        public TypeDescription GetOrAdd(NewTypeRequest request)
        {
            return _typeDescriptions.GetOrAdd(request);
        }

        public long ClearTypeCache()
        {
            long result = _typeDescriptions.Count;
            _typeDescriptions.Clear();
            return result;
        }
    }
}
