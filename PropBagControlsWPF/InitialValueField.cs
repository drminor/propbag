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
    ///     <MyNamespace:InitalValueField/>
    ///
    /// </summary>
    public class InitialValueField : Control
    {
        static InitialValueField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InitialValueField), new FrameworkPropertyMetadata(typeof(InitialValueField)));
        }


        static DependencyProperty InitialValueProperty =
            DependencyProperty.Register("InitialValue", typeof(string), typeof(InitialValueField));

        public string InitialValue
        {
            get
            {
                return (string)this.GetValue(InitialValueProperty);
            }
            set
            {
                this.SetValue(InitialValueProperty, value);
            }
        }

        static DependencyProperty SetToUndefinedProperty =
            DependencyProperty.Register("SetToUndefined", typeof(bool), typeof(InitialValueField));

        public bool SetToUndefined
        {
            get
            {
                return (bool)this.GetValue(SetToUndefinedProperty);
            }
            set
            {
                this.SetValue(SetToUndefinedProperty, value);
            }
        }

        static DependencyProperty SetToDefaultProperty =
            DependencyProperty.Register("SetToDefault", typeof(bool), typeof(InitialValueField));

        public bool SetToDefault
        {
            get
            {
                return (bool)this.GetValue(SetToDefaultProperty);
            }
            set
            {
                this.SetValue(SetToDefaultProperty, value);
            }
        }

        static DependencyProperty SetToNullProperty =
            DependencyProperty.Register("SetToNull", typeof(bool), typeof(InitialValueField));

        public bool SetToNull
        {
            get
            {
                return (bool)this.GetValue(SetToNullProperty);
            }
            set
            {
                this.SetValue(SetToNullProperty, value);
            }
        }

        static DependencyProperty SetToEmptyStringProperty =
            DependencyProperty.Register("SetToEmptyString", typeof(bool), typeof(InitialValueField));

        public bool SetToEmptyString
        {
            get
            {
                return (bool)this.GetValue(SetToEmptyStringProperty);
            }
            set
            {
                this.SetValue(SetToEmptyStringProperty, value);
            }
        }

    }
}
