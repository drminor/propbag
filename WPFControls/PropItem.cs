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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DRM.PropBag.ControlsWPF
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DRM.WPFControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DRM.WPFControls;assembly=DRM.WPFControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:PropItem/>
    ///
    /// </summary>
    public class PropItem : ItemsControl
    {
        static PropItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropItem), new FrameworkPropertyMetadata(typeof(PropItem)));
        }

        public PropItem()
        {
            //this.OnVisualChildrenChanged()
        }

        //override protected void OnVisualChildrenChanged(DependencyObject a, DependencyObject b)
        //{
        //    if(a != null && !(a is ContentPresenter))
        //    {
        //        if(! (a is InitialValueField || a is PropItem))
        //            throw new InvalidOperationException(string.Format("{0} cannot be added here.", a.ToString()));
        //    }

        //    base.OnVisualChildrenChanged(a, b);
        //}

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(PropItem), new PropertyMetadata(null));

        public string PropertyName
        {
            get
            {
                return (string)this.GetValue(PropertyNameProperty);
            }
            set
            {
                this.SetValue(PropertyNameProperty, value);
            }
        }

        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(string), typeof(PropItem));

        public string PropertyType
        {
            get
            {
                return (string)this.GetValue(PropertyTypeProperty);
            }
            set
            {
                this.SetValue(PropertyTypeProperty, value);
            }
        }

        //public static readonly DependencyProperty InitialValueFieldProperty =
        //    DependencyProperty.Register("InitialValueField", typeof(InitialValueField), typeof(PropItem));

        //public InitialValueField InitialValueField
        //{
        //    get
        //    {
        //        return (InitialValueField)this.GetValue(InitialValueFieldProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(InitialValueFieldProperty, value);
        //    }
        //}

        public static readonly DependencyProperty ExtraInfoProperty =
            DependencyProperty.Register("ExtraInfo", typeof(string), typeof(PropItem), new PropertyMetadata(null));

        public string ExtraInfo
        {
            get
            {
                return (string)this.GetValue(ExtraInfoProperty);
            }
            set
            {
                this.SetValue(ExtraInfoProperty, value);
            }
        }



    }
}
