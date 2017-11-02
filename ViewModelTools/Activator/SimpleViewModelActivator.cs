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
        public object GetNewViewModel(string resourceKey, Type typeToCreate, string fullClassName = null, IPropFactory propFactory = null)
        {
            PropModel propModel = GetPropModel(resourceKey);
            object result = GetNewViewModel(propModel, typeToCreate, fullClassName, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known only at run time.
        public object GetNewViewModel(PropModel propModel, Type typeToCreate, string fullClassName = null, IPropFactory propFactory = null)
        {
            object[] parameters = new object[] { propModel, fullClassName, propFactory };
            object result = Activator.CreateInstance(typeToCreate, parameters);
            return result;
        }

        // BaseType + ResourceKey (BaseType known at compile time.)
        public object GetNewViewModel<BT>(string resourceKey, string fullClassName = null, IPropFactory propFactory = null) where BT : class, IPropBag
        {
            PropModel propModel = GetPropModel(resourceKey);
            object result = GetNewViewModel<BT>(propModel, fullClassName, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known at compile time.)
        public object GetNewViewModel<BT>(PropModel propModel, string fullClassName = null, IPropFactory propFactory = null) where BT : class, IPropBag
        {
            object[] parameters = new object[] { propModel, fullClassName, propFactory };
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
