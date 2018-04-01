using System.Threading;
using System.Runtime.CompilerServices;

using System.ComponentModel;

using DRM.TypeSafePropertyBag; using Swhp.Tspb.PropBagAutoMapperService;
using DRM.PropBag; using DRM.TypeSafePropertyBag; using Swhp.Tspb.PropBagAutoMapperService;

namespace PropBagLib.Tests
{
    public partial class PerformanceDynModel : PropBagDyn
    {
        static public PerformanceDynModel Create(PropBagTypeSafetyMode safetyMode)
        {
            PerformanceDynModel pm = new PerformanceDynModel(safetyMode);
            pm.AddPropNoStore<int>("PropIntNoStore", null, false, null);
            pm.AddPropNoStore<string>("PropStringNoStore", null, false, null);

            return pm;
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

        new public void ClearEventSubscribers()
        {
            base.ClearEventSubscribers();
        }

    }
}
