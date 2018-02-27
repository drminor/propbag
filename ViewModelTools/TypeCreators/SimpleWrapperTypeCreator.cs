using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.PropBag.TypeWrapper;
using System;

namespace DRM.PropBag.ViewModelTools
{
    public class SimpleWrapperTypeCreator : ICreateWrapperTypes
    {
        #region Private Members

        //private string NO_PROPMODEL_LOOKUP_SERVICES = $"The {nameof(SimpleWrapperTypeCreator)} has no PropModelProvider." +
        //    $"All calls must provide a PropModel.";

        private IProvidePropModels _propModelProvider { get; }
        private ICacheWrapperTypes _wrapperTypeCachingService { get; }
        ICacheTypeDescriptions _typeDescCachingService { get; }

        #endregion

        #region Public Properties
        public bool HasPropModelLookupService => (_propModelProvider != null);
        #endregion

        #region Constructor 

        public SimpleWrapperTypeCreator(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = null;

            // This could be helpful for some diagnostic work.
            //System.Diagnostics.Debug.WriteLine(NO_PROPMODEL_LOOKUP_SERVICES);
        }

        public SimpleWrapperTypeCreator(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService,
            IProvidePropModels propModelProvider)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = propModelProvider; // ?? throw new ArgumentNullException("propModelProvider");
        }

        #endregion

        //public Type GetWrapperType(string resourceKey, Type typeToCreate)
        //{
        //    IPropModel propModel = GetPropModel(resourceKey);
        //    Type result = GetWrapperType(propModel, typeToCreate);
        //    return result;
        //}

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

        //public Type GetWrapperType<BT>(string resourceKey) where BT : class, IPropBag
        //{
        //    IPropModel propModel = GetPropModel(resourceKey);
        //    Type result = GetWrapperType<BT>(propModel);
        //    return result;
        //}

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

        //#region PropModel Lookup Support

        //private IPropModel GetPropModel(string resourceKey)
        //{
        //    if (!HasPropModelLookupService)
        //    {
        //        throw new InvalidOperationException(NO_PROPMODEL_LOOKUP_SERVICES);
        //    }

        //    IPropModel propModel = _propModelProvider.GetPropModel(resourceKey);
        //    return propModel;
        //}

        //#endregion
    }
}
