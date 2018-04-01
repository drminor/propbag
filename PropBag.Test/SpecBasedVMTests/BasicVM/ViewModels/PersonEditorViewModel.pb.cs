using DRM.PropBag;
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

    public partial class PersonEditorViewModel : PropBag
    {
        private static PropBagTypeDescriptionProvider<PersonEditorViewModel> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonEditorViewModel>();

        static PersonEditorViewModel()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonEditorViewModel));
        }

        #region Constructors

        public PersonEditorViewModel(PropModelType pm, ViewModelFactoryInterface viewModelFactory,
            IPropBagMapperService autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, viewModelFactory, autoMapperService, propFactory, fullClassName)
        {
            System.Diagnostics.Debug.WriteLine("Constructing PersonEditorViewModel -- with PropModel.");
        }

        private PersonEditorViewModel(PersonEditorViewModel copySource)
            : base(copySource)
        {
        }

        new public object Clone()
        {
            return new PersonEditorViewModel(this);
        }

        #endregion

    }
}
