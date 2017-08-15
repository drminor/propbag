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

using System.Collections.ObjectModel;

namespace DRM.PropBag.ControlsWPF
{
    public class PropBagTemplate : ItemsControl
    {
        static PropBagTemplate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropBagTemplate), new FrameworkPropertyMetadata(typeof(PropBagTemplate)));
        }


        public static readonly DependencyProperty ClassNameProperty =
        DependencyProperty.Register("ClassName", typeof(string), typeof(PropBagTemplate), new PropertyMetadata(null));

        public string ClassName
        {
            get
            {
                return (string)this.GetValue(ClassNameProperty);
            }
            set
            {
                this.SetValue(ClassNameProperty, value);
            }
        }

        public static readonly DependencyProperty NamespacesProperty =
            DependencyProperty.Register("Namespaces", typeof(NamespacesCollection), typeof(PropBagTemplate), new PropertyMetadata(new NamespacesCollection()));

        public NamespacesCollection Namespaces
        {
            get
            {
                return (NamespacesCollection)this.GetValue(NamespacesProperty);
            }
            set
            {
                this.SetValue(NamespacesProperty, value);
            }
        }


        public static readonly DependencyProperty PropsProperty =
            DependencyProperty.Register("Props", typeof(PropsCollection), typeof(PropBagTemplate), new PropertyMetadata(new PropsCollection()));

        public PropsCollection Props
        {
            get
            {
                return (PropsCollection)this.GetValue(PropsProperty);
            }
            set
            {
                this.SetValue(PropsProperty, value);
            }
        }
        
    }
}
