using System;
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
            DependencyProperty.Register("DoWhenChangedAction", typeof(DoWhenChangedAction),
                typeof(PropDoWhenChangedField), new PropertyMetadata(new DoWhenChangedAction()));

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

        static DependencyProperty MethodIsLocalProperty =
            DependencyProperty.Register("MethodIsLocal", typeof(bool), typeof(PropDoWhenChangedField));

        public bool MethodIsLocal
        {
            get
            {
                return (bool)this.GetValue(MethodIsLocalProperty);
            }
            set
            {
                this.SetValue(MethodIsLocalProperty, value);
            }
        }

        public static readonly DependencyProperty DeclaringTypeProperty =
            DependencyProperty.Register("DeclaringType", typeof(Type), typeof(PropDoWhenChangedField));

        public Type DeclaringType
        {
            get
            {
                return (Type)this.GetValue(DeclaringTypeProperty);
            }
            set
            {
                this.SetValue(DeclaringTypeProperty, value);
            }
        }

        public static readonly DependencyProperty FullClassNameProperty =
            DependencyProperty.Register("FullClassName", typeof(string), typeof(PropDoWhenChangedField));

        public string FullClassName
        {
            get
            {
                return (string)this.GetValue(FullClassNameProperty);
            }
            set
            {
                this.SetValue(FullClassNameProperty, value);
            }
        }

        public static readonly DependencyProperty InstanceKeyProperty =
            DependencyProperty.Register("InstanceKey", typeof(string), typeof(PropDoWhenChangedField));

        public string InstanceKey
        {
            get
            {
                return (string)this.GetValue(InstanceKeyProperty);
            }
            set
            {
                this.SetValue(InstanceKeyProperty, value);
            }
        }

        public static readonly DependencyProperty MethodNameProperty =
            DependencyProperty.Register("MethodName", typeof(string), typeof(PropDoWhenChangedField));

        public string MethodName
        {
            get
            {
                return (string)this.GetValue(MethodNameProperty);
            }
            set
            {
                this.SetValue(MethodNameProperty, value);
            }
        }

    }
}
