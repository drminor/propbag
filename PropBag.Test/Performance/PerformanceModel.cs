using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using PropBagLib.Tests.AutoMapperSupport;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PropBagLib.Tests
{
    public partial class PerformanceModel : PropBag
    {
        static public PerformanceModel Create(PropBagTypeSafetyMode safetyMode)
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            // TODO: AAA
            PerformanceModel pm = new PerformanceModel(safetyMode, ourHelper.StoreAccessCreator, null, propFactory_V1);

            pm.AddPropNoStore<int>("PropIntNoStore");
            pm.AddPropNoStore<string>("PropStringNoStore");

            return pm;
        }

        new public IProp<T> GetTypedProp<T>(string propertyName)
        {
            IProp<T> result = base.GetTypedProp<T>(propertyName);
            return result;
        }


        // Regular Property Definitions Used as a control
        public event PropertyChangedEventHandler PropertyChanged2;

        int _propInt;
        public int PropIntStandard
        {
            get { return _propInt; }

            set
            {
                if (_propInt != value)
                {

                    _propInt = value;
                    OnPropertyChanged2();
                }
            }
        }


        string _propString;
        public string PropStringStandard
        {
            get { return _propString; }

            set
            {
                if (_propString != value)
                {

                    _propString = value;
                    OnPropertyChanged2();
                }
            }
        }

        int _propIntNoStore;
        public int PropIntNoStore
        {
            get
            {
                return _propIntNoStore;
            }
            set
            {
                SetIt<int>(value, ref _propIntNoStore, nameof(PropIntNoStore));
            }
        }  

        
        string _propStringNoStore;
        public string PropStringNoStore
        {
            get
            {
                return _propStringNoStore;
            }
            set
            {
                SetIt(value, ref _propStringNoStore, nameof(PropStringNoStore));
            }
        }

        protected void OnPropertyChanged2([CallerMemberName]string propertyName = null)
        {
            Interlocked.CompareExchange(ref PropertyChanged2, null, null)?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

        }
    }
}
