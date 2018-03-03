using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels
{
    using PropNameType = String;
        using PropModelType = IPropModel<String>;

    // This code could be created by a VS Extension.
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class PersonVM : PropBag, ICloneable
    {
        public PersonVM(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, propFactory, fullClassName)
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
