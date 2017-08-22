using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Reflection;

using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;

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
        static public void StandUpViewModels(Panel root, FrameworkElement modelHolder)
        {
            Type propModelType = typeof(DRM.PropBag.ControlModel.PropModel);

            IEnumerable<PropBagTemplate> propBagTemplates = root.Children.OfType<PropBagTemplate>();

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
                }
            }
        }
    }
}
