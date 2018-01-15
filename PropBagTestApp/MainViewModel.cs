﻿using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Drawing;

namespace PropBagTestApp
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class MainViewModel : PropBag, INotifyPropertyChanged, INotifyPropertyChanging
    {
        public bool PropFirstDidChange;
        public bool PropMyStringDidChange;
        public bool PropMyPointDidChange;

        //private MainViewModel() { } // Shows that if no default constructor is available, the one that takes a single byte is use.

        // If it not desirable to provide a public, default, parameterless constructor, 
        // a consructor that takes a single byte can be used instead.
        // NOTE: Neither of these constructors is required if an instance of this class already exists from the proerty 
        // marked with the PropBagInstanceAttribute.
        // An instance of this class must be available so that we create an instance of a Action<T,T> delegate.
        //public MainViewModel(byte dummy) : base(dummy) {}

        /// <summary>
        /// Constructor used by View to create with properties
        /// </summary>
        /// <param name="pm"></param>
        public MainViewModel(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory)
            : base(pm, storeAccessCreator, propFactory, fullClassName: null)
        {
            this.PropFirstDidChange = false;
            this.PropMyStringDidChange = false;
            this.PropMyPointDidChange = false;
        }

        //public new object this[string propertyName]
        //{
        //    get { return base.GetValWithType(propertyName); }
        //    set { base.SetIt(value, propertyName); }
        //}

        private void DoWhenFirstChanges(bool oldVal, bool newVal)
        {
            this.PropFirstDidChange = true;
        }

        private void DoWhenMyStringChanges(string oldVal, string newVal)
        {
            this.PropMyStringDidChange = true;
        }

        private void DoWhenMyPointChanges(Point oldVal, Point newVal)
        {
            PropMyPointDidChange = true;
        }

        
    }
}
