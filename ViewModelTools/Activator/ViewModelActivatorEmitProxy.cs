using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeWrapper;
using System;

namespace DRM.ViewModelTools
{
    public class ViewModelActivatorEmitProxy<T> : IViewModelActivator<T> where T : class, IPropBag
    {
        #region Private Members
        private IPropModelProvider _propModelProvider { get; }
        private ICacheWrapperTypes _wrapperTypeCachingService { get; }
        ICacheTypeDescriptions _typeDescCachingService { get; }

        private string NO_PBT_CONVERSION_SERVICE_MSG = $"The {nameof(ViewModelActivatorStandard<T>)} has no PropModelProvider." +
                    $"All calls must provide a PropModel.";
        #endregion

        #region Public Properties
        public bool HasPbTConversionService => (_propModelProvider != null);
        #endregion

        #region Constructor 

        public ViewModelActivatorEmitProxy(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = null;

            System.Diagnostics.Debug.WriteLine(NO_PBT_CONVERSION_SERVICE_MSG);
        }

        public ViewModelActivatorEmitProxy(
            ICacheWrapperTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService,
            IPropModelProvider propModelProvider)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
            _propModelProvider = propModelProvider ?? throw new ArgumentNullException("propModelProvider");
        }

        #endregion

        #region IViewModelActivator interface

        public T GetNewViewModel(string resourceKey, IPropFactory propFactory)
        {
            PropModel propModel = GetPropModel(resourceKey);

            T result = GetNewViewModel(propModel, propFactory);

            return result;
        }

        public T GetNewViewModel(PropModel propModel, IPropFactory propFactory)
        {
            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeof(T), null));

            T result = _wrapperTypeCachingService.GetOrAdd(td) as T;

            return result;
        }

        public T GetNewViewModel(string resourceKey, IPropFactory propFactory, Type baseType)
        {
            PropModel propModel = GetPropModel(resourceKey);

            IViewModelActivator<T> us = (IViewModelActivator<T>)this;
            T result = us.GetNewViewModel(propModel, propFactory, baseType);

            return result;
        }

        public T GetNewViewModel(PropModel propModel, IPropFactory propFactory, Type baseType)
        {
            if (!baseType.IsPropBagBased())
            {
                throw new InvalidOperationException($"Type: {baseType.Name} must derive from IPropBag.");
            }

            // TODO: verify that baseType derives from T.

            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, baseType, null));

            T result = _wrapperTypeCachingService.GetOrAdd(td) as T;

            return result;
        }

        // BaseType + ClassName (BaseType known at compile time.)
        T IViewModelActivator<T>.GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory)
        {
            PropModel propModel = GetPropModel(resourceKey);

            IViewModelActivator<T> us = (IViewModelActivator<T>)this;
            T result = us.GetNewViewModel<BT>(propModel, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known at compile time.)
        T IViewModelActivator<T>.GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory)
        {
            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeof(BT), null));

            T result = _wrapperTypeCachingService.GetOrAdd(td) as T;

            return result;
        }

        private PropModel GetPropModel(string resourceKey)
        {
            if (!HasPbTConversionService)
            {
                throw new InvalidOperationException(NO_PBT_CONVERSION_SERVICE_MSG);
            }

            PropModel propModel = _propModelProvider.GetPropModel(resourceKey);
            return propModel;
        }

        #endregion
    }
}
