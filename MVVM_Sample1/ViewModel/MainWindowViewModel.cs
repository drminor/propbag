using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBagWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Threading;

namespace MVVMApplication.ViewModel
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public class MainWindowViewModel : PropBag
    {
        public event EventHandler<EventArgs> RequestClose;

        public MainWindowViewModel(IPropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, autoMapperService, propFactory, fullClassName)
        {
            //System.Diagnostics.Debug.WriteLine("Beginning to construct MainWindowViewModel -- From PropModel.");

            //IHaveADbContext dBActivator = new DBActivator<PersonDB>(System.Environment.SpecialFolder.CommonApplicationData);
            //PersonDB personDb = (PersonDB)dBActivator.DbContext;
            //PersonDAL b = new PersonDAL(personDb);
            //SetIt(b, "Business");

            //System.Diagnostics.Debug.WriteLine("Completed Constructing MainWindowViewModel -- From PropModel.");
        }

        public MainWindowViewModel(MainWindowViewModel copySource)
            : base(copySource)
        {
        }

        public void CloseTheWindow()
        {
            Interlocked.CompareExchange(ref RequestClose, null, null)?.Invoke(this, EventArgs.Empty);
        }

        public RelayCommand Close => new RelayCommand(CloseIt);

        private void CloseIt(object o)
        {
            CloseTheWindow();
        }
    }
}
