using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.ComponentModel;

namespace MVVM_Sample1.ViewModel
{
    // This code could be created by a VS Extension.
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public partial class PersonVM : PropBag, ICloneable
    {
        private static PropBagTypeDescriptionProvider<PersonVM> _typeDescriptionProvider = new PropBagTypeDescriptionProvider<PersonVM>();

        static PersonVM()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(PersonVM));
        }

        public PersonVM(PropModelType pm, ViewModelFactoryInterface viewModelFactory,
            IPropFactory propFactory, string fullClassName)
            : base(pm, viewModelFactory, propFactory, fullClassName)
        {
            //PropBagTypeDescriptionProvider<PersonVM> tdp = RegisterTypeDescriptorProvider<PersonVM>(pm);
            //pm.TypeDescriptionProvider = tdp;
            //TypeDescriptor.AddProvider(tdp, this);

            //List<string> pNamesFromOurProvider = tdp.GetPropertyDescriptors(this).Select(x => x.Name).ToList();

            //List<string> pNamesFromOurPropModel = pm.PropertyDescriptors.Select(x => x.Name).ToList();

            //List<string> pNames = TypeInspectorUtility.GetPropertyNames(this);

            //System.Diagnostics.Debug.WriteLine("PersonVM is being created with a PropModel.");
        }

        // This constructor is required for AutoMapperSupport when using "Emit_Proxy."
        // TODO: Consider having DRM.PropBag.TypeWrapper.SimpleWrapperTypeEmitter create this
        // constructor if one was not declared.
        public PersonVM(PersonVM copySource)
            : base(copySource)
        {
            //PropBagTypeDescriptionProvider<PersonVM> tdp = RegisterTypeDescriptorProvider<PersonVM>(_propModel);
            //_propModel.TypeDescriptionProvider = tdp;

            //TypeDescriptor.AddProvider(tdp, this);
            //System.Diagnostics.Debug.WriteLine("PersonVM is being created from an existing instance of a PersonVM.");
        }

        // This constructor is required for AutoMapperSupport when using "Extra_Members."
        // TODO: Consider having DRM.PropBag.TypeWrapper.SimpleWrapperTypeEmitter create this
        // constructor if one was not declared.
        override public object Clone()
        {
            return new PersonVM(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
