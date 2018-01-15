using DRM.TypeSafePropertyBag;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
{
    public class TypeInfoField : ItemsControl
    {
        static TypeInfoField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TypeInfoField), new FrameworkPropertyMetadata(typeof(TypeInfoField)));
        }

        public TypeInfoField()
        {
        }

        public static readonly DependencyProperty MemberTypeProperty =
            DependencyProperty.Register("MemberType", typeof(Type), typeof(TypeInfoField));

        public Type MemberType
        {
            get
            {
                return (Type)this.GetValue(MemberTypeProperty);
            }
            set
            {
                this.SetValue(MemberTypeProperty, value);
            }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(TypeInfoField), new PropertyMetadata(null));

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
            DependencyProperty.Register("FullyQualifiedTypeName", typeof(string), typeof(TypeInfoField), new PropertyMetadata(null));

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
            DependencyProperty.Register("CollectionType", typeof(WellKnownCollectionTypeEnum?), typeof(TypeInfoField),
                new PropertyMetadata((WellKnownCollectionTypeEnum?) null));

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
            DependencyProperty.Register("TypeParameter1", typeof(Type), typeof(TypeInfoField));

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
            DependencyProperty.Register("TypeParameter2", typeof(Type), typeof(TypeInfoField));

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
            DependencyProperty.Register("TypeParameter3", typeof(Type), typeof(TypeInfoField));

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

        TypeInfoCollection _childern;
        public TypeInfoCollection TypeParameters
        {
            get
            {
                if(_childern == null)
                {
                    _childern = new TypeInfoCollection();
                }
                return _childern;
            }

        }

    }
}
