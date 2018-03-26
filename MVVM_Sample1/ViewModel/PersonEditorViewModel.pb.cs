using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MVVM_Sample1.ViewModel
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
            IPropFactory propFactory, string fullClassName)
            : base(pm, viewModelFactory, propFactory, fullClassName)
        {
            //PropBagTypeDescriptionProvider<PersonEditorViewModel> tdp = RegisterTypeDescriptorProvider<PersonEditorViewModel>(pm);
            //pm.TypeDescriptionProvider = tdp;
            //TypeDescriptor.AddProvider(tdp, this);

            //List<string> pNamesFromOurProvider = tdp.GetPropertyDescriptors(this).Select(x => x.Name).ToList();

            //List<string> pNamesFromOurPropModel = pm.PropertyDescriptors.Select(x => x.Name).ToList();

            //List<string> pNames = TypeInspectorUtility.GetPropertyNames(this);

            _commands = new List<RelayCommand>();

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

        protected override void Dispose(bool disposing)
        {
            // TODO: Instead of having to cleanup our RelayCommands,
            // have the RelayCommand class use weak references.

            // See: https://blogs.msdn.microsoft.com/nathannesbit/2009/05/29/wpf-icommandsource-implementations-leak-memory/

            foreach (RelayCommand rc in _commands)
            {
                //rc.CanExecuteChanged -= AddPersonRelayCmd_CanExecuteChanged;
                rc.Clear();
            }

            _commands.Clear();

            base.Dispose(disposing);
        }

        #endregion

    }
}
