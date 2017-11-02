using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF.WPFHelpers;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBag.ControlsWPF
{
    public class ViewModelActivator<T> : IViewModelActivator<T> where T: class, IPropBag
    {
        private IPropModelProvider _propModelProvider { get; }
        private IPropBagTemplateProvider _propBagTemplateProvider { get; }
        private string NO_FIND_PBT_WITH_JUST_KEY_MSG = null;
        private string NO_PBT_LOOKUP_RESOURCES_MSG = null;
        private string NO_PROPMODEL_LOOKUP_SERVICES = null;

        public bool CanFindPropBagTemplateWithJustKey => (_propModelProvider?.CanFindPropBagTemplateWithJustKey != false) || _propBagTemplateProvider?.CanFindPropBagTemplateWithJustKey != false;
        public bool HasPbtLookupResources => (_propModelProvider?.HasPbtLookupResources != false || _propBagTemplateProvider != null);
        public bool HasPbTConversionService => (_propModelProvider != null);

        #region Constructor 

        public ViewModelActivator(IPropModelProvider propModelProvider)
        {
            _propModelProvider = propModelProvider;
            _propBagTemplateProvider = null;

            CheckInputs(_propModelProvider, _propBagTemplateProvider);
        }

        public ViewModelActivator(IPropModelProvider propModelProvider, IPropBagTemplateProvider propBagTemplateProvider)
        {
            _propModelProvider = propModelProvider;
            _propBagTemplateProvider = propBagTemplateProvider;

            CheckInputs(_propModelProvider, _propBagTemplateProvider);
        }

        #endregion

        #region Activation Services

        // Just Key
        public T GetNewViewModel(string resourceKey, IPropFactory propFactory)
        {
            if(!HasPbtLookupResources)
            {
                throw new InvalidOperationException(NO_PBT_LOOKUP_RESOURCES_MSG);
            }
            if (!CanFindPropBagTemplateWithJustKey)
            {
                throw new InvalidOperationException(NO_FIND_PBT_WITH_JUST_KEY_MSG);
            }
            if(!HasPbTConversionService)
            {
                throw new InvalidCastException(NO_PROPMODEL_LOOKUP_SERVICES);
            }

            // Fetch the propModel using just the ResourceKey
            PropModel propModel = GetPropModel(resourceKey, _propModelProvider, _propBagTemplateProvider);

            T result = GetNewViewModel(propModel, propFactory);
            return result;
        }

        // Dictionary
        public T GetNewViewModel(ResourceDictionary rd, string resourceKey, IPropFactory propFactory)
        {
            if (!HasPbtLookupResources)
            {
                throw new InvalidOperationException(NO_PBT_LOOKUP_RESOURCES_MSG);
            }
            if (!HasPbTConversionService)
            {
                throw new InvalidCastException(NO_PROPMODEL_LOOKUP_SERVICES);
            }

            // Fetch the propModel using just the ResourceKey
            PropModel propModel = GetPropModel(rd, resourceKey, _propModelProvider, _propBagTemplateProvider);

            T result = GetNewViewModel(propModel, propFactory);
            return result;
        }

        // PropBagTemplate
        public T GetNewViewModel(PropBagTemplate pbt, IPropFactory propFactory)
        {
            if (!HasPbTConversionService)
            {
                throw new InvalidCastException(NO_PROPMODEL_LOOKUP_SERVICES);
            }

            PropModel propModel = _propModelProvider.GetPropModel(pbt);

            T result = GetNewViewModel(propModel, propFactory);
            return result;
        }

        // PropModel
        public T GetNewViewModel(PropModel propModel, IPropFactory propFactory) 
        {
            T result = (T)Activator.CreateInstance(typeof(T), propModel, propFactory);
            return result;
        }

        // BaseType + PropModel (BaseType known at compile time.)
        public T GetNewViewModel<BT>(PropModel propModel, IPropFactory propFactory) where BT: class, IPropBag
        {
            T result = (T)Activator.CreateInstance(typeof(BT), propModel, propFactory);
            return result;
        }


        public T GetNewViewModel(Type baseType, PropModel propModel, IPropFactory propFactory)
        {
            if(!baseType.IsPropBagBased())
            {
                throw new InvalidOperationException($"Type: {baseType.Name} must derive from IPropBag.");
            }

            // TODO: verify that baseType derives from T.

            T result = (T)Activator.CreateInstance(baseType, propModel, propFactory);
            return result;
        }

        #endregion

        #region View Model Locator Services

        private PropModel GetPropModel(string resourceKey,
            IPropModelProvider propModelProvider, IPropBagTemplateProvider propBagTemplateProvider = null)
        {
            PropModel result;

            if (propBagTemplateProvider?.CanFindPropBagTemplateWithJustKey != false && propModelProvider != null)
            {
                PropBagTemplate pbt = propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                result = propModelProvider.GetPropModel(pbt);
            }
            else if (propModelProvider.HasPbtLookupResources)
            {
                result = propModelProvider.GetPropModel(resourceKey);
            }
            else
            {
                throw new InvalidOperationException("This ViewModelActivator does not have the necessary resources to locate a PropBagTemplate using just an instance key.");
            }

            return result;
        }

        private PropModel GetPropModel(ResourceDictionary rd, string resourceKey,
            IPropModelProvider propModelProvider, IPropBagTemplateProvider propBagTemplateProvider)
        {
            PropModel result;

            if (propBagTemplateProvider != null && propModelProvider != null)
            {
                PropBagTemplate pbt = propBagTemplateProvider.GetPropBagTemplate(rd, resourceKey);
                result = propModelProvider.GetPropModel(pbt);
            }
            else if (propModelProvider.HasPbtLookupResources)
            {
                result = propModelProvider.GetPropModel(rd, resourceKey);
            }
            else
            {
                throw new InvalidOperationException("This ViewModelActivator does not have the necessary resources to locate a PropBagTemplate.");
            }

            return result;
        }

        void CheckInputs(IPropModelProvider propModelProvider, IPropBagTemplateProvider propBagTemplateProvider)
        {
            // Prepare Exception Messages in case "Just a ResourceKey.."
            if(!CanFindPropBagTemplateWithJustKey)
            {
                NO_FIND_PBT_WITH_JUST_KEY_MSG = $"The {nameof(ViewModelActivator<T>)} has been given a {nameof(propModelProvider)} and a {nameof(propBagTemplateProvider)}, however neither has the necessary resources. " +
                    "All calls must include a reference to a ResourceDictionary.";
                System.Diagnostics.Debug.WriteLine(NO_FIND_PBT_WITH_JUST_KEY_MSG);
            }

            // Prepare Exception Message for must have PropModel. (No PropBagTemplateServices, and ResourceDictionary + ResourceKey access available.)
            if (!HasPbtLookupResources)
            {
                NO_PBT_LOOKUP_RESOURCES_MSG = $"The {nameof(ViewModelActivator<T>)} has not been given a {nameof(propBagTemplateProvider)} and the {nameof(propModelProvider)} was not provisioned with the necessary resources. " +
                    $"All calls must provide a {nameof(PropBagTemplate)} or a {nameof(PropModel)}.";
                System.Diagnostics.Debug.WriteLine(NO_PBT_LOOKUP_RESOURCES_MSG);
            }
            else
            {
                // If both a PropBagTemplateProvider and a PropModelProvider were supplied and the PropModelProvider has
                // the ability to lookup PropBagTemplates either by ResourceKey only or by ResourceDictionary and ResourceKey,
                // write to the debug output...
                // a notice that PropBagTemplate services will be used be used first.
                if (_propModelProvider.CanFindPropBagTemplateWithJustKey && _propBagTemplateProvider.CanFindPropBagTemplateWithJustKey)
                {
                    System.Diagnostics.Debug.WriteLine($"The {nameof(ViewModelActivator<T>)} has been given a {nameof(propModelProvider)} and a {nameof(propBagTemplateProvider)} and both have resources. " +
                        "The resources of the propBagTemplateProvider will be used exclusively.");
                }
            }

            // Prepare Exception Message in case "A ResourceDictionary and ResourceKey are supplied, but no PBT Service available.
            if (!HasPbTConversionService)
            {
                NO_PROPMODEL_LOOKUP_SERVICES = $"The {nameof(ViewModelActivator<T>)} has no PropModelProvider." +
                    $"All calls must provide a PropModel.";
            }


        }

        #endregion
    }
}
