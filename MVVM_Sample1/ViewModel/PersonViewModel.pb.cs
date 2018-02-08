﻿using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;

namespace MVVMApplication.ViewModel
{
    // This code could be created by a VS Extension.
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class PersonVM : PropBag, ICloneable
    {
        public PersonVM(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, propFactory, fullClassName)
        {
        }

        // This constructor is required for AutoMapperSupport when using "Emit_Proxy."
        // TODO: Consider having DRM.PropBag.TypeWrapper.SimpleWrapperTypeEmitter create this
        // constructor if one was not declared.
        public PersonVM(PersonVM copySource)
            : base(copySource)
        {
        }

        // This constructor is required for AutoMapperSupport when using "Extra_Members."
        // TODO: Consider having DRM.PropBag.TypeWrapper.SimpleWrapperTypeEmitter create this
        // constructor if one was not declared.
        override public object Clone()
        {
            return new PersonVM(this);
        }
    }
}