using DRM.PropBag.ControlModel;
using DRM.PropBag.ViewModelBuilder;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    // TODO: Convert these to methods from static to instance so that the
    // source of these services can be determined at run-time so that we can test the units that
    // rely on this service in isolation.
    public class ViewModelGenerator
    {
        /// <summary>
        /// Processes each PropBagTemplate by creating the class instance, registering the properties
        /// an assigning the new instance to the property marked with the PropBagInstanceAttribute Attribute
        /// that "belongs" to the PropBagTemplate.
        /// This should be called in the view's constructor.
        /// If the DataContext is will be assigned via code, it should be done after this method is called.
        /// </summary>
        /// <param name="root">The UI Element at which to begin looking for PropBagTemplate elements.</param>
        /// <param name="modelHolder">The window or user control that "hosts" the property that is marked with
        /// the PropBagInstanceAttribute Attribute</param>
        static public Dictionary<string, BoundPropBag> StandUpViewModels(Panel root, FrameworkElement modelHolder, IModuleBuilderInfo modelBuilder = null)
        {
            IEnumerable<PropBagTemplate> propBagTemplates = root.Children.OfType<PropBagTemplate>();
            Dictionary<string, BoundPropBag> boundTemplates = new Dictionary<string, BoundPropBag>();

            // Use the ModuleBuilder supplied, or if it's null, the default ModuleBuilder
            // provided by the PropBag AutoMapperSupport library.
            IModuleBuilderInfo typeEmitter = GetEmitter(modelBuilder);

            foreach (PropBagTemplate pbt in propBagTemplates)
            {
                // Build a control model from the XAML contents of the template.

                // TODO: we need another service that only takes PBT's and doesn't need a PBPProvider.
                PropModel pm = new PropModelProvider().GetPropModel(pbt);

                if (pm != null)
                {
                    // Get a reference to the property that access the class that needs to be created.
                    Type thisType = modelHolder.GetType();
                    PropertyInfo classAccessor = ReflectionHelpers.GetPropBagClassProperty(thisType, pm.ClassName, pm.InstanceKey);

                    Type dtViewModelType = classAccessor.PropertyType;

                    // Instantiate the target ViewModel
                    //ReflectionHelpers.CreateTargetAndAssign(modelHolder, classAccessor, propModelType, pm);

                    // Use the name of the class that dervives from IPropBag
                    // as the basis of the name of the new wrapper type.
                    TypeDescription typeDescription = new TypeDescriptionProvider().BuildTypeDesc(pm, dtViewModelType);

                    Type proxyType = typeEmitter.BuildVmProxyClass(typeDescription);

                    var newInstance = GetNewInstance(proxyType, pm);

                    classAccessor.SetValue(modelHolder, newInstance);

                    // Record each "live" template.
                    BoundPropBag boundPB = new BoundPropBag(pm, dtViewModelType, proxyType, classAccessor);
                    boundTemplates.Add(pm.InstanceKey, boundPB);
                }
            }
            return boundTemplates;
        }

        private static ITypeSafePropBag GetNewInstance(Type proxyType, PropModel pm)
        {
            bool isPropBagMin = proxyType.IsPropBagBased();
            
            if(isPropBagMin)
            {
                IPropBag newInstance = (IPropBag)System.Activator.CreateInstance(proxyType, new object[] { pm });
                return newInstance;
            }
            else
            {
                throw new ApplicationException("Target view model must derive from IPropBag.");
            }
        }

        public static IModuleBuilderInfo GetEmitter(IModuleBuilderInfo provider)
        {
            return provider ?? new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;
        }

        private static Type CreateWrapper(TypeDescription td, IModuleBuilderInfo modBuilderInfo = null)
        {
            // If the caller did not supply a ModuleBuilderInfo object, then use the default one 
            // provided by the WrapperGenLib
            IModuleBuilderInfo builderInfoToUse = modBuilderInfo ?? new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;

            Type emittedType = builderInfoToUse.BuildVmProxyClass(td);

            System.Diagnostics.Debug.WriteLine($"Created Type: {emittedType.FullName}");

            return emittedType;
        }

    }
}
