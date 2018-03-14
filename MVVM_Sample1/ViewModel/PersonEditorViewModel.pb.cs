using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.ComponentModel;

namespace MVVM_Sample1.ViewModel
{
    using PropModelType = IPropModel<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class PersonEditorViewModel : PropBag
    {
        private static PropBagTypeDescriptionProvider<PersonEditorViewModel> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonEditorViewModel>();

        static PersonEditorViewModel()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonEditorViewModel));
        }

        #region Constructors

        public PersonEditorViewModel(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, propFactory, fullClassName)
        {
            //PropBagTypeDescriptionProvider<PersonEditorViewModel> tdp = RegisterTypeDescriptorProvider<PersonEditorViewModel>(pm);
            //pm.TypeDescriptionProvider = tdp;
            //TypeDescriptor.AddProvider(tdp, this);

            //List<string> pNamesFromOurProvider = tdp.GetPropertyDescriptors(this).Select(x => x.Name).ToList();

            //List<string> pNamesFromOurPropModel = pm.PropertyDescriptors.Select(x => x.Name).ToList();

            //List<string> pNames = TypeInspectorUtility.GetPropertyNames(this);

            System.Diagnostics.Debug.WriteLine("Constructing PersonEditorViewModel -- with PropModel.");
        }

        private PersonEditorViewModel(PersonEditorViewModel copySource)
            : base(copySource)
        {
            //PropBagTypeDescriptionProvider<PersonEditorViewModel> tdp = RegisterTypeDescriptorProvider<PersonEditorViewModel>(_propModel);
            //_propModel.TypeDescriptionProvider = tdp;

            //TypeDescriptor.AddProvider(tdp, this);
        }

        new public object Clone()
        {
            return new PersonEditorViewModel(this);
        }

        #endregion

    }
}
