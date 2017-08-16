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
