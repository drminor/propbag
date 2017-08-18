﻿using System;
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

        public static byte TEST_FLAG = 0xff;

        static PropBagTemplate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropBagTemplate), new FrameworkPropertyMetadata(typeof(PropBagTemplate)));
        }

        public PropBagTemplate()
        {
            this.Namespaces = new NamespacesCollection();
            this.Props = new PropsCollection();
        }

        static DependencyProperty DeriveFromPubPropBagProperty =
            DependencyProperty.Register("DeriveFromPubPropBag", typeof(bool), typeof(PropItem), new PropertyMetadata(true));

        public bool DeriveFromPubPropBag
        {
            get
            {
                return (bool)this.GetValue(DeriveFromPubPropBagProperty);
            }
            set
            {
                this.SetValue(DeriveFromPubPropBagProperty, value);
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
            DependencyProperty.Register("OutPutNameSpace", typeof(string), typeof(PropBagTemplate), new PropertyMetadata(null));

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

        public static readonly DependencyProperty TypeSafetyModeProperty =
            DependencyProperty.Register("TypeSafetyMode", typeof(PropBagTypeSafetyMode), typeof(PropBagTemplate), new PropertyMetadata(PropBagTypeSafetyMode.AllPropsMustBeRegistered));

        public PropBagTypeSafetyMode TypeSafetyMode
        {
            get
            {
                return (PropBagTypeSafetyMode)this.GetValue(TypeSafetyModeProperty);
            }
            set
            {
                this.SetValue(TypeSafetyModeProperty, value);
            }
        }

        static DependencyProperty DeferMethodRefResolutionProperty =
            DependencyProperty.Register("DeferMethodRefResolution", typeof(bool), typeof(PropItem), new PropertyMetadata(true));

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
            DependencyProperty.Register("RequireExplicitInitialValue", typeof(bool), typeof(PropItem), new PropertyMetadata(true));

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
            DependencyProperty.Register("PropFactory", typeof(AbstractPropFactory), typeof(PropItem), new PropertyMetadata(null));

        public AbstractPropFactory PropFactory
        {
            get
            {
                return (AbstractPropFactory)this.GetValue(PropFactoryProperty);
            }
            set
            {
                this.SetValue(PropFactoryProperty, value);
            }
        }

        //public static readonly DependencyProperty NamespacesProperty =
        //    DependencyProperty.Register("Namespaces", typeof(NamespacesCollection), typeof(PropBagTemplate), new PropertyMetadata(new NamespacesCollection()));

        public NamespacesCollection Namespaces { get; set; }
        //{
        //    get
        //    {
        //        return (NamespacesCollection)this.GetValue(NamespacesProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(NamespacesProperty, value);
        //    }
        //}


        //public static readonly DependencyProperty PropsProperty =
        //    DependencyProperty.Register("Props", typeof(PropsCollection), typeof(PropBagTemplate), new PropertyMetadata(new PropsCollection()));

        public PropsCollection Props { get; set; }
        //{
        //    get
        //    {
        //        return (PropsCollection)this.GetValue(PropsProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(PropsProperty, value);
        //    }
        //}
        
    }
}