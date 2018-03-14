using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeDescriptors;
using System;
using System.ComponentModel;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels
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

        public MainWindowViewModel(PropModelType pm, PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, propFactory, fullClassName)
        {
            //System.Diagnostics.Debug.WriteLine("Beginning to construct MainWindowViewModel -- From PropModel.");


            //System.Diagnostics.Debug.WriteLine("Completed Constructing MainWindowViewModel -- From PropModel.");
        }

        public MainWindowViewModel(MainWindowViewModel copySource)
            : base(copySource)
        {
        }
    }
}
