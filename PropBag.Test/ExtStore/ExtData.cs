using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Runtime.CompilerServices;

using System.ComponentModel;

namespace PropBagLib.Tests
{
    public class ExtData
    {
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

        protected void OnPropertyChanged2([CallerMemberName]string propertyName = null)
        {
            PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged2, null, null);

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
