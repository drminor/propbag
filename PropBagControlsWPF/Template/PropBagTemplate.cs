using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class PropBagTemplate : ItemsControl
    {
        public static byte TEST_FLAG = 0xff;

        #region Constructors

        static PropBagTemplate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropBagTemplate), new FrameworkPropertyMetadata(typeof(PropBagTemplate)));
        }

        public PropBagTemplate()
        {
        }

        #endregion

        #region Activation Info

        public static readonly DependencyProperty DeriveFromClassModeProperty =
            DependencyProperty.Register
            (
                "DeriveFromClassMode",
                typeof(DeriveFromClassModeEnum),
                typeof(PropBagTemplate),
                new PropertyMetadata
                (
                    DeriveFromClassModeEnum.PropBag
                )
            );

        public DeriveFromClassModeEnum DeriveFromClassMode
        {
            get
            {
                object test = this.GetValue(DeriveFromClassModeProperty);
                return (DeriveFromClassModeEnum)test; //  CoerceTypeSafetyMode(this, test);
            }
            set
            {
                //object test = CoerceTypeSafetyMode(this, value);
                this.SetValue(TypeSafetyModeProperty, value);
            }
        }

        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register("TargetType", typeof(Type), typeof(PropBagTemplate));

        public Type TargetType
        {
            get
            {
                return (Type)this.GetValue(TargetTypeProperty);
            }
            set
            {
                this.SetValue(TargetTypeProperty, value);
            }
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

        public static readonly DependencyProperty OutPutNameSpaceProperty =
            DependencyProperty.Register("OutPutNameSpace", typeof(string), typeof(PropBagTemplate),
                new PropertyMetadata(null));

        public string OutPutNameSpace
        {
            get
            {
                return (string)this.GetValue(OutPutNameSpaceProperty);
            }
            set
            {
                this.SetValue(OutPutNameSpaceProperty, value);
            }
        }

        #endregion

        #region Other Properties

        public static readonly DependencyProperty TypeSafetyModeProperty =
            DependencyProperty.Register
            (
                "TypeSafetyMode",
                typeof(PropBagTypeSafetyMode),
                typeof(PropBagTemplate),
                new PropertyMetadata
                (
                    PropBagTypeSafetyMode.None,
                    new PropertyChangedCallback(OnTypeSafetyModeChanged)
                    //, new CoerceValueCallback(CoerceTypeSafetyMode)
                )
            );

        public PropBagTypeSafetyMode TypeSafetyMode
        {
            get
            {
                object test = this.GetValue(TypeSafetyModeProperty);
                return (PropBagTypeSafetyMode)CoerceTypeSafetyMode(this, test);
            }
            set
            {
                object test = CoerceTypeSafetyMode(this, value);
                this.SetValue(TypeSafetyModeProperty, test);
            }
        }

        private static void OnTypeSafetyModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            object o = e.OldValue;
            object n = e.NewValue;
        }

        private static object CoerceTypeSafetyMode(DependencyObject d, object value)
        {
            if(value != null)
            {
                Type vType = value.GetType();

                if (vType == typeof(string))
                {
                    EnumConverter enumConverter = new EnumConverter(typeof(PropBagTypeSafetyMode));
                    object test = enumConverter.ConvertFromString((string)value);
                    if (test is PropBagTypeSafetyMode pbtsm)
                    {
                        return pbtsm;
                    }
                }
                else
                {
                    if (vType == typeof(PropBagTypeSafetyMode))
                    {
                        return value;
                    }
                }
            }

            return PropBagTypeSafetyMode.None;
        }

        static DependencyProperty DeferMethodRefResolutionProperty =
            DependencyProperty.Register("DeferMethodRefResolution", typeof(bool), typeof(PropBagTemplate), new PropertyMetadata(true));

        public bool DeferMethodRefResolution
        {
            get
            {
                return (bool)this.GetValue(DeferMethodRefResolutionProperty);
            }
            set
            {
                this.SetValue(DeferMethodRefResolutionProperty, value);
            }
        }

        static DependencyProperty RequireExplicitInitialValueProperty =
            DependencyProperty.Register("RequireExplicitInitialValue", typeof(bool), typeof(PropBagTemplate), new PropertyMetadata(true));

        public bool RequireExplicitInitialValue
        {
            get
            {
                return (bool)this.GetValue(RequireExplicitInitialValueProperty);
            }
            set
            {
                this.SetValue(RequireExplicitInitialValueProperty, value);
            }
        }

        // TODO: need to create a typeconverter for AbstractPropFactory.
        static DependencyProperty PropFactoryProperty =
            DependencyProperty.Register("PropFactory", typeof(IPropFactory), typeof(PropBagTemplate), new PropertyMetadata(null));

        public IPropFactory PropFactory
        {
            get
            {
                return (IPropFactory)this.GetValue(PropFactoryProperty);
            }
            set
            {
                this.SetValue(PropFactoryProperty, value);
            }
        }

        //public static readonly DependencyProperty NamespacesProperty =
        //    DependencyProperty.Register("Namespaces", typeof(NamespacesCollection), typeof(PropBagTemplate), new PropertyMetadata(new NamespacesCollection()));

        NamespacesCollection _namespaces;
        public NamespacesCollection Namespaces
        {
            get
            {
                if(_namespaces == null)
                {
                    _namespaces = new NamespacesCollection();
                }
                return _namespaces;
            }
        }


        //public static readonly DependencyProperty PropsProperty =
        //    DependencyProperty.Register("Props", typeof(PropsCollection), typeof(PropBagTemplate), new PropertyMetadata(new PropsCollection()));

        PropsCollection _props;
        public PropsCollection Props
        {
            get
            {
                if(_props == null)
                {
                    _props = new PropsCollection();
                }
                return _props;
            }
        }

        #endregion

        #region Type Support

        public string FullClassName
        {
            get
            {
                IEnumerable<string> namespaces = this.Namespaces.Select(x => x.Namespace.Trim());
                return GetFullClassName(namespaces, this.ClassName.Trim());
            }
        }

        private string GetFullClassName(IEnumerable<string> namespaces, string className)
        {
            if(namespaces == null)
            {
                return className;
            }
            else
            {
                string separator = ".";
                string result = $"{string.Join(separator, namespaces)}{separator}{className}";

                return result;
            }

        }

        #endregion
    }
}

