using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.Reflection;


using DRM.PropBag.ControlModel;
using DRM.PropBag.ViewModelBuilder;

namespace DRM.PropBag.ControlsWPF
{
    public class ViewModelGenerator
    {
        public const string DEFAULT_NAMESPACE_NAME = "PropBagWrappers";

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
            //Type propModelType = typeof(DRM.PropBag.ControlModel.PropModel);

            IEnumerable<PropBagTemplate> propBagTemplates = root.Children.OfType<PropBagTemplate>();
            Dictionary<string, BoundPropBag> boundTemplates = new Dictionary<string, BoundPropBag>();

            foreach (PropBagTemplate pbt in propBagTemplates)
            {
                // Build a control model from the XAML contents of the template.
                DRM.PropBag.ControlModel.PropModel pm = pbt.GetPropBagModel();
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
                    TypeDescription typeDescription = BuildTypeDesc(dtViewModelType, null, pm);

                    IModuleBuilderInfo typeEmitter = GetEmitter(modelBuilder);

                    Type proxyType = typeEmitter.GetWrapperType(typeDescription);

                    //Type[] implementedTypes = proxyType.GetTypeInfo().ImplementedInterfaces.ToArray();

                    IPropBag newInstance = (IPropBag) Activator.CreateInstance(proxyType, new object[] { pm });

                    classAccessor.SetValue(modelHolder, newInstance);

                    // Record each "live" template.
                    BoundPropBag boundPB = new BoundPropBag(dtViewModelType, pm, proxyType);
                    boundTemplates.Add(pm.InstanceKey, boundPB);
                }
            }
            return boundTemplates;
        }

        private static IModuleBuilderInfo GetEmitter(IModuleBuilderInfo provided)
        {
            return provided ?? new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtViewModelType">The type (usually known at compile-time for which the proxy is being created.</param>
        /// <param name="namespaceName">The namespace to use when creating the proxy.</param>
        /// <param name="pm">The propmodel from which to get the list of properties to define for the proxy.</param>
        /// <returns></returns>
        private static TypeDescription BuildTypeDesc(Type dtViewModelType, string namespaceName,  PropModel pm)
        {
            string nsName = namespaceName ?? DEFAULT_NAMESPACE_NAME;

            TypeName tn = new TypeName(dtViewModelType.Name, nsName);

            IEnumerable<PropertyDescription> propDescs = pm.GetPropertyDescriptions();

            TypeDescription td = new TypeDescription(tn, dtViewModelType, propDescs);
            return td;
        }

        private static Type CreateWrapper(TypeDescription td, IModuleBuilderInfo modBuilderInfo = null)
        {
            // If the caller did not supply a ModuleBuilderInfo object, then use the default one 
            // provided by the WrapperGenLib
            IModuleBuilderInfo builderInfoToUse = modBuilderInfo ?? new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;

            Type emittedType = builderInfoToUse.GetWrapperType(td);

            System.Diagnostics.Debug.WriteLine($"Created Type: {emittedType.FullName}");

            return emittedType;
        }

    }
}
