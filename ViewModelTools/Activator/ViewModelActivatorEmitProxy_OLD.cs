using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeWrapper;
using System;

namespace DRM.ViewModelTools
{
    public class ViewModelActivatorEmitProxy : IViewModelActivator 
    {
        #region Private Members
        private IPropModelProvider _propModelProvider { get; }
        private ICacheWrapperTypes _wrapperTypeCachingService { get; }
        ICacheTypeDescriptions _typeDescCachingService { get; }

        private string NO_PROPMODEL_LOOKUP_SERVICES = $"The {nameof(ViewModelActivatorEmitProxy)} has no PropModelProvider." +
                    $"All calls must provide a PropModel.";
        #endregion

        #region Public Properties
        public bool HasPbTConversionService => (_propModelProvider != null);
        public bool HasTypeCreationService => true;
        #endregion

        #region Constructor 

        public ViewModelActivatorEmitProxy(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = null;

            System.Diagnostics.Debug.WriteLine(NO_PROPMODEL_LOOKUP_SERVICES);
        }

        public ViewModelActivatorEmitProxy(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService,
            IPropModelProvider propModelProvider)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = propModelProvider; // ?? throw new ArgumentNullException("propModelProvider");
        }

        #endregion

        #region IViewModelActivator interface

        public object GetNewViewModel(string resourceKey, Type typeToCreate, IPropFactory propFactory = null)
        {
            PropModel propModel = GetPropModel(resourceKey);

            object result = GetNewViewModel(propModel, typeToCreate, propFactory);
            return result;
        }

        public object GetNewViewModel(PropModel propModel, Type typeToCreate, IPropFactory propFactory = null)
        {
            Type newWrapperType = GetWrapperType(propModel, typeToCreate);

            object result = Activator.CreateInstance(newWrapperType, propModel, propFactory);
            return result;
        }

        // BaseType + ClassName (BaseType known at compile time.)
        public object GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory = null) where BT : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);

            object result = GetNewViewModel<BT>(propModel, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known at compile time.)
        public object GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory = null) where BT : class, IPropBag
        {
            //TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeof(BT), null));
            //Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);

            Type newWrapperType = GetWrapperType<BT>(propModel);

            object result = Activator.CreateInstance(newWrapperType, propModel, propFactory);
            return result;
        }



        #endregion
    }
}
