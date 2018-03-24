using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.TypeWrapper;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels
{
    using PropModelType = IPropModel<String>;

    // This code could be created by a VS Extension.
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class PersonVM : PropBag, ICloneable
    {
        private static PropBagTypeDescriptionProvider<PersonVM> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonVM>();

        static PersonVM()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonVM));
        }

        public PersonVM(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
            IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fullClassName)
        {
            System.Diagnostics.Debug.WriteLine("PersonVM is being created with a PropModel.");

            //////IList<string> pNamesFromOurProvider = TypeInspectorUtility.GetPropertyNames
            //////    (_typeDescriptionProvider.GetTypeDescriptor(this));

            ////ICustomTypeDescriptor ourCustomTypeDescriptor = _typeDescriptionProvider.GetTypeDescriptor(this);
            ////IList<string> pNamesFromOurProvider = TypeInspectorUtility.GetPropertyNames
            ////    (ourCustomTypeDescriptor);

            //IList<string> pNamesFromOurPropModel = TypeInspectorUtility.GetPropertyNames
            //    (pm.CustomTypeDescriptor);

            ////IList<string> pNamesFromAppDomain_WT = TypeInspectorUtility.GetPropertyNames
            ////    (typeof(PersonVM), this);

            ////IList<string> pNamesFromAppDomain_WT = TypeInspectorUtility.GetPropertyNames
            ////    (this.GetType(), this);

            //IList<string> pNamesFromAppDomain = TypeInspectorUtility.GetPropertyNames
            //    (this);
        }

        // This constructor is required for AutoMapperSupport when using "Emit_Proxy."
        // TODO: Consider having DRM.PropBag.TypeWrapper.SimpleWrapperTypeEmitter create this
        // constructor if one was not declared.
        public PersonVM(PersonVM copySource)
            : base(copySource)
        {
            System.Diagnostics.Debug.WriteLine("PersonVM is being created from an existing instance of a PersonVM.");

            //IList<string> pNamesFromOurProvider = TypeInspectorUtility.GetPropertyNames
            //    (_typeDescriptionProvider.GetTypeDescriptor(this));

            //IList<string> pNamesFromOurPropModel = TypeInspectorUtility.GetPropertyNames
            //    (copySource._propModel.CustomTypeDescriptor);

            //IList<string> pNamesFromAppDomain_WT = TypeInspectorUtility.GetPropertyNames
            //    (typeof(PersonVM), this);

            //IList<string> pNamesFromAppDomain = TypeInspectorUtility.GetPropertyNames
            //    (this);
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
