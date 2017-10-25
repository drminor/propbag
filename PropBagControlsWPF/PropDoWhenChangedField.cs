using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class PropDoWhenChangedField : Control
    {
        static PropDoWhenChangedField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropDoWhenChangedField), new FrameworkPropertyMetadata(typeof(PropDoWhenChangedField)));
        }

        static DependencyProperty DoWhenChangedActionProperty =
            DependencyProperty.Register("DoWhenChangedAction", typeof(DoWhenChangedAction), typeof(PropDoWhenChangedField), new PropertyMetadata(new DoWhenChangedAction()));

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
