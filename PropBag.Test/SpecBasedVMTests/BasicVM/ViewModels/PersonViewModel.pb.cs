﻿using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using Swhp.Tspb.PropBagAutoMapperService;
using System;
using System.ComponentModel;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    // This code could be created by a VS Extension.

    public partial class PersonVM : PropBag, ICloneable
    {
        private static PropBagTypeDescriptionProvider<PersonVM> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonVM>();

        static PersonVM()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonVM));
        }

        public PersonVM(PropModelType pm, ViewModelFactoryInterface viewModelFactory,
            IPropBagMapperService autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, viewModelFactory, autoMapperService, propFactory, fullClassName)
        {
            //System.Diagnostics.Debug.WriteLine("PersonVM is being created with a PropModel.");

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
