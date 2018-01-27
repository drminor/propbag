using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
{
    public class NamespaceItem : Control
    {
        static NamespaceItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NamespaceItem), new FrameworkPropertyMetadata(typeof(NamespaceItem)));
        }

        public static readonly DependencyProperty NamespaceProperty =
            DependencyProperty.Register("Namespace", typeof(string), typeof(PropBagTemplate), new PropertyMetadata(null));

        public string Namespace
        {
            get
            {
                return (string)this.GetValue(NamespaceProperty);
            }
            set
            {
                this.SetValue(NamespaceProperty, value);
            }
        }
    }
}
