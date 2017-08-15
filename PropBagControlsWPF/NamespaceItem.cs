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
