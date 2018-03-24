using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.TypeWrapper;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.ComponentModel;

namespace MVVM_Sample1.ViewModel
{
    using PropModelType = IPropModel<String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class MainWindowViewModel : PropBag
    {
        private static PropBagTypeDescriptionProvider<MainWindowViewModel> _typeDescriptionProvider 
            = new PropBagTypeDescriptionProvider<MainWindowViewModel>();

        static MainWindowViewModel()
        {
            TypeDescriptor.AddProvider(_typeDescriptionProvider, typeof(MainWindowViewModel));
        }

        public MainWindowViewModel(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator,
            IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, wrapperTypeCreator, propFactory, fullClassName)
        {
            //System.Diagnostics.Debug.WriteLine("Beginning to construct MainWindowViewModel -- From PropModel.");

            //IHaveADbContext dBActivator = new DBActivator<PersonDB>(System.Environment.SpecialFolder.CommonApplicationData);
            //PersonDB personDb = (PersonDB)dBActivator.DbContext;
            //PersonDAL b = new PersonDAL(personDb);
            //SetIt(b, "Business");

            //PropBagTypeDescriptionProvider<MainWindowViewModel> tdp = RegisterTypeDescriptorProvider<MainWindowViewModel>(pm);
            //pm.TypeDescriptionProvider = tdp;
            //TypeDescriptor.AddProvider(tdp, this);

            //List<string> pNamesFromOurProvider = tdp.GetPropertyDescriptors(this).Select(x => x.Name).ToList();

            //List<string> pNamesFromOurPropModel = pm.PropertyDescriptors.Select(x => x.Name).ToList();

            //List<string> pNames = TypeInspectorUtility.GetPropertyNames(this);

            System.Diagnostics.Debug.WriteLine("Completed Constructing MainWindowViewModel -- From PropModel.");
        }

        public MainWindowViewModel(MainWindowViewModel copySource)
            : base(copySource)
        {
            //PropBagTypeDescriptionProvider<MainWindowViewModel> tdp = RegisterTypeDescriptorProvider<MainWindowViewModel>(_propModel);
            //_propModel.TypeDescriptionProvider = tdp;

            //TypeDescriptor.AddProvider(tdp, this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
