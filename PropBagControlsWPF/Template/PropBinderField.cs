using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class PropBinderField : Control
    {
        static PropBinderField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropBinderField), new FrameworkPropertyMetadata(typeof(PropBinderField)));
        }

        //static DependencyProperty TargetPropertyProperty =
        //    DependencyProperty.Register("TargetProperty", typeof(string), typeof(PropBinderField), new PropertyMetadata(null));

        //public string TargetProperty
        //{
        //    get
        //    {
        //        return (string)this.GetValue(TargetPropertyProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(TargetPropertyProperty, value);
        //    }
        //}

        static DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(PropBinderField), new PropertyMetadata(null));

        public string Path
        {
            get
            {
                return (string)this.GetValue(PathProperty);
            }
            set
            {
                this.SetValue(PathProperty, value);
            }
        }

    }
}
