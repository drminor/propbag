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

namespace DRM.PropBag.ControlsWPF
{
    public class PropComparerField : Control
    {
        static PropComparerField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropComparerField), new FrameworkPropertyMetadata(typeof(PropComparerField)));
        }

        static DependencyProperty ComparerFuncProperty =
            DependencyProperty.Register("ComparerFunc", typeof(PropComparerFunc), typeof(PropComparerField), new PropertyMetadata(new PropComparerFunc()));

        [TypeConverter(typeof(ComparerFuncTypeConverter))]
        public PropComparerFunc ComparerFunc
        {
            get
            {
                var x = (PropComparerFunc)this.GetValue(ComparerFuncProperty);
                return x;
            }
            set
            {
                this.SetValue(ComparerFuncProperty, value);
            }
        }

        static DependencyProperty UseRefEqualityProperty =
            DependencyProperty.Register("UseRefEquality", typeof(bool), typeof(PropItem), new PropertyMetadata(true));

        public bool UseRefEquality
        {
            get
            {
                return (bool)this.GetValue(UseRefEqualityProperty);
            }
            set
            {
                this.SetValue(UseRefEqualityProperty, value);
            }


            // TODO: Create User Control for ComparerImpl (of type Delegate / IEquatable<T>
            // And its attendant TypeConverter.
        }
    }
}
