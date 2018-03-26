using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.ComponentModel;

namespace MVVM_Sample1.ViewModel
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public partial class PersonCollectionViewModel : PropBag
    {
        private static PropBagTypeDescriptionProvider<PersonCollectionViewModel> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonCollectionViewModel>();

        static PersonCollectionViewModel()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonCollectionViewModel));
        }

        public PersonCollectionViewModel(PropModelType pm, ViewModelFactoryInterface viewModelFactory, IPropFactory propFactory, string fullClassName)
            : base(pm, viewModelFactory, propFactory, fullClassName)
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
