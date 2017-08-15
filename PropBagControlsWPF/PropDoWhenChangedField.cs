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

using System.ComponentModel;

using DRM.PropBag.ControlModel;

namespace DRM.PropBag.ControlsWPF
{
    public class PropDoWhenChangedField : Control
    {
        static PropDoWhenChangedField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropDoWhenChangedField), new FrameworkPropertyMetadata(typeof(PropDoWhenChangedField)));
        }


        static DependencyProperty DoWhenChangedActionProperty =
            DependencyProperty.Register("DoWhenChangedAction", typeof(DoWhenChangedAction), typeof(PropDoWhenChangedField));

        [TypeConverter(typeof(DuoActionTypeConverter))]
        public DoWhenChangedAction DoWhenChangedAction
        {
            get
            {
                var x = (DoWhenChangedAction)this.GetValue(DoWhenChangedActionProperty);
                return x;
            }
            set
            {
                this.SetValue(DoWhenChangedActionProperty, value);
            }
        }

        static DependencyProperty TestDoWProperty =
            DependencyProperty.Register("TestDoW", typeof(string), typeof(PropDoWhenChangedField));

        public string TestDoW
        {
            get
            {
                return (string)this.GetValue(TestDoWProperty);
            }
            set
            {
                this.SetValue(TestDoWProperty, value);
            }
        }

        static DependencyProperty DoAfterNotifyProperty =
            DependencyProperty.Register("DoAfterNotify", typeof(bool), typeof(PropDoWhenChangedField));

        public bool DoAfterNotify
        {
            get
            {
                return (bool)this.GetValue(DoAfterNotifyProperty);
            }
            set
            {
                this.SetValue(DoAfterNotifyProperty, value);
            }
        }
    }
}
