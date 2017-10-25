using DRM.PropBag.ControlModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class PropTypeInfoField : ItemsControl
    {
        static PropTypeInfoField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropTypeInfoField), new FrameworkPropertyMetadata(typeof(PropTypeInfoField)));
        }

        public PropTypeInfoField()
        {
            this.Children = new PropTypeInfoCollection();
        }

        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(Type), typeof(PropTypeInfoField));

        public Type PropertyType
        {
            get
            {
                return (Type)this.GetValue(PropertyTypeProperty);
            }
            set
            {
                this.SetValue(PropertyTypeProperty, value);
            }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(PropTypeInfoField), new PropertyMetadata(null));

        public string TypeName
        {
            get
            {
                return (string)this.GetValue(TypeNameProperty);
            }
            set
            {
                this.SetValue(TypeNameProperty, value);
            }
        }

        public static readonly DependencyProperty FullyQualifiedTypeNameProperty =
            DependencyProperty.Register("FullyQualifiedTypeName", typeof(string), typeof(PropTypeInfoField), new PropertyMetadata(null));

        public string FullyQualifiedTypeName
        {
            get
            {
                return (string)this.GetValue(FullyQualifiedTypeNameProperty);
            }
            set
            {
                this.SetValue(FullyQualifiedTypeNameProperty, value);
            }
        }

        public static readonly DependencyProperty CollectionTypeProperty =
            DependencyProperty.Register("TypeSafetyMode", typeof(WellKnownCollectionTypeEnum?), typeof(PropTypeInfoField), new PropertyMetadata((WellKnownCollectionTypeEnum?) null));

        public WellKnownCollectionTypeEnum? CollectionType
        { 
            get
            {
                return (WellKnownCollectionTypeEnum?)this.GetValue(CollectionTypeProperty);
            }
            set
            {
                this.SetValue(CollectionTypeProperty, value);
            }
        }

        public static readonly DependencyProperty TypeParameter1Property =
            DependencyProperty.Register("TypeParameter1", typeof(Type), typeof(PropTypeInfoField));

        public Type TypeParameter1
        {
            get
            {
                return (Type)this.GetValue(TypeParameter1Property);
            }
            set
            {
                this.SetValue(TypeParameter1Property, value);
            }
        }

        public static readonly DependencyProperty TypeParameter2Property =
            DependencyProperty.Register("TypeParameter2", typeof(Type), typeof(PropTypeInfoField));

        public Type TypeParameter2
        {
            get
            {
                return (Type)this.GetValue(TypeParameter2Property);
            }
            set
            {
                this.SetValue(TypeParameter2Property, value);
            }
        }

        public static readonly DependencyProperty TypeParameter3Property =
            DependencyProperty.Register("TypeParameter3", typeof(Type), typeof(PropTypeInfoField));

        public Type TypeParameter3
        {
            get
            {
                return (Type)this.GetValue(TypeParameter3Property);
            }
            set
            {
                this.SetValue(TypeParameter3Property, value);
            }
        }



        public PropTypeInfoCollection Children { get; set; }

    }
}
