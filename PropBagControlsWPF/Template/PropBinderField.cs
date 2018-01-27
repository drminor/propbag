using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
{
    public class PropBinderField : Control
    {
        static PropBinderField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropBinderField), new FrameworkPropertyMetadata(typeof(PropBinderField)));
        }

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

        public static readonly DependencyProperty MapperRequestResourceKeyProperty =
            DependencyProperty.Register("MapperRequestResourceKey", typeof(string), typeof(PropBinderField), new PropertyMetadata(null));

        public string MapperRequestResourceKey
        {
            get
            {
                return (string)this.GetValue(MapperRequestResourceKeyProperty);
            }
            set
            {
                this.SetValue(MapperRequestResourceKeyProperty, value);
            }
        }

    }
}
