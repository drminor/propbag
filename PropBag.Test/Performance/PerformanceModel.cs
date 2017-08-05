using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Runtime.CompilerServices;

using System.ComponentModel;

using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class PerformanceModel : PropBag
    {
        static public PerformanceModel Create(PropBagTypeSafetyMode safetyMode)
        {
            PerformanceModel pm = new PerformanceModel(safetyMode);
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
                SetIt<int>(value, ref _propIntNoStore);
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
                SetIt(value, ref _propStringNoStore);
            }
        }  

        // For Loose Testing
        public new object this[string key]
        {
            get { return base[key]; }
            set { base[key] = value; }
        }

        protected void OnPropertyChanged2([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged2, null, null);

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
