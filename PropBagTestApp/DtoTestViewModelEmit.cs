﻿using DRM.PropBag;
using DRM.PropBag.ControlModel;

namespace PropBagTestApp
{
    public class DtoTestViewModelEmit : PubPropBag
    {
        PropModel _pm;

        public DtoTestViewModelEmit() { } // Shows that if no default constructor is available, the one that takes a single byte is use.

        // If it not desirable to provide a public, default, parameterless constructor, 
        // a consructor that takes a single byte can be used instead.
        // NOTE: Neither of these constructors is required if an instance of this class already exists from the proerty 
        // marked with the PropBagInstanceAttribute.
        // An instance of this class must be available so that we create an instance of a Action<T,T> delegate.
        public DtoTestViewModelEmit(byte dummy) : base(dummy) {}

        /// <summary>
        /// Constructor used by View to create with properties
        /// </summary>
        /// <param name="pm"></param>
        public DtoTestViewModelEmit(PropModel pm) : base(pm)
        {
            // Save a reference to the model used to defined our properties.
            _pm = pm;
        }
        private void DoWhenProductIdChanges(bool oldVal, bool newVal)
        {
            System.Diagnostics.Debug.WriteLine("ProductId was changed.");
        }
    }
}