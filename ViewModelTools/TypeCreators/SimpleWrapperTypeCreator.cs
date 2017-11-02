using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeWrapper;
using System;

namespace DRM.ViewModelTools
{
    public class SimpleWrapperTypeCreator : ICreateWrapperType
    {
        #region Private Members

        private string NO_PROPMODEL_LOOKUP_SERVICES = $"The {nameof(SimpleWrapperTypeCreator)} has no PropModelProvider." +
            $"All calls must provide a PropModel.";

        private IPropModelProvider _propModelProvider { get; }
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

            System.Diagnostics.Debug.WriteLine(NO_PROPMODEL_LOOKUP_SERVICES);
        }

        public SimpleWrapperTypeCreator(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService,
            IPropModelProvider propModelProvider)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = propModelProvider; // ?? throw new ArgumentNullException("propModelProvider");
        }

        #endregion

        public Type GetWrapperType(string resourceKey, Type typeToCreate)
        {
            PropModel propModel = GetPropModel(resourceKey);
            Type result = GetWrapperType(propModel, typeToCreate);
            return result;
        }

        public Type GetWrapperType(PropModel propModel, Type typeToCreate)
        {
            if (!typeToCreate.IsPropBagBased())
            {
                throw new InvalidOperationException($"Type: {typeToCreate.Name} must derive from IPropBag.");
            }

            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeToCreate, null));
            Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);

            return newWrapperType;
        }

        public Type GetWrapperType<BT>(string resourceKey) where BT : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);
            Type result = GetWrapperType<BT>(propModel);
            return result;
        }

        public Type GetWrapperType<BT>(PropModel propModel) where BT : class, IPropBag
        {

            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeof(BT), null));
            Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);

            return newWrapperType;
        }

        #region PropModel Lookup Support
        private PropModel GetPropModel(string resourceKey)
        {
            if (!HasPropModelLookupService)
            {
                throw new InvalidOperationException(NO_PROPMODEL_LOOKUP_SERVICES);
            }

            PropModel propModel = _propModelProvider.GetPropModel(resourceKey);
            return propModel;
        }
        #endregion
    }
}
