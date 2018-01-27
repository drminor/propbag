using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
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

        }
    }
}
