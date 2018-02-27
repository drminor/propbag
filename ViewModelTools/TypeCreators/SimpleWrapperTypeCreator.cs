using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.PropBag.TypeWrapper;
using System;

namespace DRM.PropBag.ViewModelTools
{
    public class SimpleWrapperTypeCreator : ICreateWrapperTypes
    {
        #region Private Members

        private ICacheWrapperTypes _wrapperTypeCachingService { get; }
        ICacheTypeDescriptions _typeDescCachingService { get; }

        #endregion

        #region Constructor 

        public SimpleWrapperTypeCreator
            (
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService
            )
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
        }

        #endregion

        public Type GetWrapperType(IPropModel propModel, Type typeToCreate)
        {
            if (!typeToCreate.IsPropBagBased())
            {
                throw new InvalidOperationException($"Type: {typeToCreate.Name} must derive from IPropBag.");
            }

            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeToCreate, null));
            Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);

            return newWrapperType;
        }

        public Type GetWrapperType<BT>(IPropModel propModel) where BT : class, IPropBag
        {
            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeof(BT), null));
            Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);

            return newWrapperType;
        }

        // TODO: Note: This class hold two caches, however only one contributes to the result.
        public long ClearTypeCache()
        {
           long numTypeDescriptionsThatWereCached = _typeDescCachingService.ClearTypeCache();
           return _wrapperTypeCachingService.ClearTypeCache();
        }
    }
}
