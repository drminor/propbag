using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.ViewModelTools
{
    public class ViewModelActivatorStandard : IViewModelActivator
    {
        #region Private Members
        private IPropModelProvider _propModelProvider { get; }

        private string NO_PBT_CONVERSION_SERVICE_MSG = $"The {nameof(ViewModelActivatorStandard)} has no PropModelProvider." +
                    $"All calls must provide a PropModel.";
        #endregion

        #region Public Properties
        public bool HasPbTConversionService => (_propModelProvider != null);
        #endregion

        #region Constructor 

        public ViewModelActivatorStandard()
        {
            System.Diagnostics.Debug.WriteLine(NO_PBT_CONVERSION_SERVICE_MSG);
        }

        public ViewModelActivatorStandard(IPropModelProvider propModelProvider)
        {
            _propModelProvider = propModelProvider ?? throw new ArgumentNullException("propModelProvider");
        }

        #endregion

        #region IViewModelActivator interface

        // BaseType + ResourceKey (BaseType known only at run time.
        public object GetNewViewModel(string resourceKey, IPropFactory propFactory, Type typeToCreate)
        {
            PropModel propModel = GetPropModel(resourceKey);
            object result = GetNewViewModel(propModel, propFactory, typeToCreate);
            return result;
        }

        // BaseType + PropModel (BaseType known only at run time.
        public object GetNewViewModel(PropModel propModel, IPropFactory propFactory, Type typeToCreate)
        {
            object result = Activator.CreateInstance(typeToCreate, propModel, propFactory);
            return result;
        }

        // BaseType + ResourceKey (BaseType known at compile time.)
        public BT GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory) where BT : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);
            BT result = GetNewViewModel<BT>(propModel, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known at compile time.)
        public BT GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory) where BT : class, IPropBag
        {
            BT result = (BT)Activator.CreateInstance(typeof(BT), propModel, propFactory);
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
