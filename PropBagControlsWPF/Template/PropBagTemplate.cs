using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
{
    using PSServiceSingletonProviderInterface = IProvidePropStoreServiceSingletons<UInt32, String>;

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

        
        public static readonly DependencyProperty PropStoreServiceProviderTypeProperty =
            DependencyProperty.Register("PropStoreServiceProviderType", typeof(Type), typeof(PropBagTemplate));

        public Type PropStoreServiceProviderType
        {
            get
            {
                return (Type)this.GetValue(PropStoreServiceProviderTypeProperty);
            }
            set
            {
                this.SetValue(PropStoreServiceProviderTypeProperty, value);
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

        public static readonly DependencyProperty AutoMapperServiceProviderTypeProperty =
            DependencyProperty.Register("AutoMapperServiceProviderType", typeof(Type), typeof(PropBagTemplate));

        public Type AutoMapperServiceProviderType
        {
            get
            {
                return (Type)this.GetValue(AutoMapperServiceProviderTypeProperty);
            }
            set
            {
                this.SetValue(AutoMapperServiceProviderTypeProperty, value);
            }
        }

        static DependencyProperty PropFactoryProviderTypeProperty =
            DependencyProperty.Register("PropFactoryProviderType", typeof(Type), typeof(PropBagTemplate), new PropertyMetadata(null));

        public Type PropFactoryProviderType
        {
            get
            {
                return (Type)this.GetValue(PropFactoryProviderTypeProperty);
            }
            set
            {
                this.SetValue(PropFactoryProviderTypeProperty, value);
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

        #region PropStoreService Support

        IPropFactory _propFactory;
        public IPropFactory PropFactory
        {
            get
            {
                if(_propFactory == null)
                {
                    Func<IPropFactory> propFactoryCreator = PropFactoryCreator;

                    if(propFactoryCreator != null)
                    {
                        _propFactory = propFactoryCreator();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"The PropFactory for PropModel for class: {FullClassName} is null.");
                    }
                }
                return _propFactory;
            }
        }

        public Func<IPropFactory> PropFactoryCreator
        {
            get
            {
                if (PropStoreServiceProviderType == null) return null;

                PSServiceSingletonProviderInterface propStoreServices = GetPropStoreServices(this.PropStoreServiceProviderType);

                IProvideAutoMappers autoMapperProvider;
                if(AutoMapperServiceProviderType == null)
                {
                    autoMapperProvider = null;
                }
                else
                {
                    autoMapperProvider = GetAutoMapperService(this.AutoMapperServiceProviderType);
                }
                
                Func<IPropFactory> result = GetPropFactoryCreator(this.PropFactoryProviderType, propStoreServices, autoMapperProvider);
                return result;
            }
        }

        private IProvideAutoMappers GetAutoMapperService(Type autoMapperServiceProviderType)
        {
            if (autoMapperServiceProviderType == null) return null;

            IAMServiceRef serviceRef = (IAMServiceRef)Activator.CreateInstance(autoMapperServiceProviderType);

            IProvideAutoMappers result = serviceRef.AutoMapperProvider;

            return result;
        }


        private PSServiceSingletonProviderInterface GetPropStoreServices(Type propStoreServiceProviderType)
        {
            if (propStoreServiceProviderType == null) return null;

            IPSServiceRef serviceRef = (IPSServiceRef) Activator.CreateInstance(propStoreServiceProviderType);

            PSServiceSingletonProviderInterface result = serviceRef.PropStoreServices;

            return result;
        }

        private Func<IPropFactory> GetPropFactoryCreator(Type propFactoryProviderType, PSServiceSingletonProviderInterface propStoreServices, IProvideAutoMappers autoMapperProvider)
        {
            if(propFactoryProviderType == null)
            {
                return null;
            }
            else
            {
                object factoryCreator = Activator.CreateInstance(propFactoryProviderType, propStoreServices, autoMapperProvider);
                IProvideAPropFactoryCreator funcProvider = (IProvideAPropFactoryCreator)factoryCreator;
                Func<IPropFactory> result = funcProvider.GetNewPropFactory;
                return result;
            }
        }

        #endregion
    }
}

