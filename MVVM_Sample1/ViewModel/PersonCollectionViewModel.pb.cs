using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.TypeWrapper;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MVVM_Sample1.ViewModel
{
    using PropModelType = IPropModel<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class PersonCollectionViewModel : PropBag
    {
        private static PropBagTypeDescriptionProvider<PersonCollectionViewModel> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonCollectionViewModel>();

        static PersonCollectionViewModel()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonCollectionViewModel));
        }

        public PersonCollectionViewModel(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
            IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fullClassName)
        {
            System.Diagnostics.Debug.WriteLine("Constructing PersonCollectionViewModel -- with PropModel.");

            //IList<string> pNamesFromOurProvider = TypeInspectorUtility.GetPropertyNames
            //    (_typeDescriptionProvider.GetTypeDescriptor(this));

            //IList<string> pNamesFromOurPropModel = TypeInspectorUtility.GetPropertyNames
            //    (pm.CustomTypeDescriptor);

            //IList<string> pNamesFromAppDomain_WT = TypeInspectorUtility.GetPropertyNames
            //    (typeof(PersonCollectionViewModel), this);

            //IList<string> pNamesFromAppDomain = TypeInspectorUtility.GetPropertyNames
            //    (this);
        }

        protected PersonCollectionViewModel(PersonCollectionViewModel copySource)
            : base(copySource)
        {
        }

        new public object Clone()
        {
            return new PersonCollectionViewModel(this);
        }
    }
}
