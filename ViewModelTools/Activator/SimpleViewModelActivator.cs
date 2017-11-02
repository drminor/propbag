using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.ViewModelTools
{
    public class SimpleViewModelActivator : IViewModelActivator
    {
        #region Private Members

        private string NO_PROPMODEL_LOOKUP_SERVICES = $"The {nameof(SimpleViewModelActivator)} has no PropModelProvider." +
                    $"All calls must provide a PropModel.";

        private IPropModelProvider _propModelProvider { get; }

        #endregion

        #region Public Properties
        public bool HasPropModelLookupService => (_propModelProvider != null);
        #endregion

        #region Constructor 

        public SimpleViewModelActivator()
        {
            System.Diagnostics.Debug.WriteLine(NO_PROPMODEL_LOOKUP_SERVICES);
        }

        public SimpleViewModelActivator(IPropModelProvider propModelProvider)
        {
            _propModelProvider = propModelProvider ?? throw new ArgumentNullException("propModelProvider");
        }

        #endregion

        #region IViewModelActivator interface

        // BaseType + ResourceKey (BaseType known only at run time.
        public object GetNewViewModel(string resourceKey, Type typeToCreate, IPropFactory propFactory = null)
        {
            PropModel propModel = GetPropModel(resourceKey);
            object result = GetNewViewModel(propModel, typeToCreate, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known only at run time.
        public object GetNewViewModel(PropModel propModel, Type typeToCreate, IPropFactory propFactory = null)
        {
            object[] parameters = new object[] { propModel, propFactory };
            object result = Activator.CreateInstance(typeToCreate, parameters);
            return result;
        }

        // BaseType + ResourceKey (BaseType known at compile time.)
        public object GetNewViewModel<BT>(string resourceKey, IPropFactory propFactory = null) where BT : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);
            object result = GetNewViewModel<BT>(propModel, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known at compile time.)
        public object GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory = null) where BT : class, IPropBag
        {
            object[] parameters = new object[] { propModel, propFactory };
            object result = Activator.CreateInstance(typeof(BT), parameters);
            return result;
        }

        private PropModel GetPropModel(string resourceKey)
        {
            if (!HasPropModelLookupService)
            {
                throw new InvalidOperationException(NO_PROPMODEL_LOOKUP_SERVICES);
            }

            PropModel propModel = _propModelProvider.GetPropModel(resourceKey);
            return propModel;
        }

        public Type GetWrapperType(PropModel propModel, Type typeToCreate)
        {
            throw new NotImplementedException();
        }

        public Type GetWrapperType<BT>(PropModel propModel) where BT: class, IPropBag
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
