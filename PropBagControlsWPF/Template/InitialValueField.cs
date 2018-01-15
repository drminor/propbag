using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
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

        static DependencyProperty CreateNewProperty =
            DependencyProperty.Register("CreateNew", typeof(bool), typeof(InitialValueField));

        public bool CreateNew
        {
            get
            {
                return (bool)this.GetValue(CreateNewProperty);
            }
            set
            {
                this.SetValue(CreateNewProperty, value);
            }
        }

        static DependencyProperty PropBagResourceKeyProperty =
            DependencyProperty.Register("PropBagResourceKey", typeof(string), typeof(InitialValueField));

        public string PropBagResourceKey
        {
            get
            {
                return (string)this.GetValue(PropBagResourceKeyProperty);
            }
            set
            {
                this.SetValue(PropBagResourceKeyProperty, value);
            }
        }

        public bool AreWeValid()
        {
            int cnt = 0;
            if (!string.IsNullOrEmpty(this.InitialValue)) cnt++;

            if (SetToUndefined) if (++cnt > 1) return false;
            if (SetToDefault) if (++cnt > 1) return false;
            if (SetToNull) if (++cnt > 1) return false;
            if (SetToEmptyString) if (++cnt > 1) return false;
            if (CreateNew) if (++cnt > 1) return false;

            if (PropBagResourceKey != null && !CreateNew) return false;

            return cnt == 1;
        }

    }
}
