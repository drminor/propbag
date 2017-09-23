using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.Reflection;

using DRM.PropBag;
using DRM.PropBag.ControlModel;

namespace DRM.PropBag.ControlsWPF
{
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
        static public Dictionary<string, BoundPropBag> StandUpViewModels(Panel root, FrameworkElement modelHolder)
        {
            Type propModelType = typeof(DRM.PropBag.ControlModel.PropModel);

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

                    // Instantiate the target ViewModel
                    ReflectionHelpers.CreateTargetAndAssign(modelHolder, classAccessor, propModelType, pm);

                    // Record each "live" template.
                    BoundPropBag boundPB = new BoundPropBag(classAccessor.PropertyType, pm);
                    boundTemplates.Add(pm.InstanceKey, boundPB);
                }
            }
            return boundTemplates;
        }

        //static public void CreateMap(MyModel mm, DtoTestViewModel vm)
        //{
        //    var config = new AutoMapper.MapperConfiguration(cfg => cfg.CreateMap<MyModel, DtoTestViewModel>());

        //    var mapper = config.CreateMapper();

        //    mapper.Map<MyModel, DtoTestViewModel>(mm, vm);
        //}

    }
}
