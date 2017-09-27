using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.ViewModelBuilder
{
    public class SampleViewModel : PropBagBase
    {
        public SampleViewModel(byte dummy)
            : base(dummy) { } // Don't call init, instances created here are not supposed to be operational.

        public SampleViewModel()
            : this(PropBagTypeSafetyMode.None, null) { }

        public SampleViewModel(PropBagTypeSafetyMode typeSafetyMode)
            : this(typeSafetyMode, null) { }

        public SampleViewModel(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory)
            : base(typeSafetyMode, thePropFactory) { Init(); }

        public SampleViewModel(PropModel pm)
            : base(pm) { Init(); }

        private void Init()
        {
            SampleString = "Hello";
        }

        public string SampleString { get; set; }

        #region Hard Coded Version of what the View Model Builder will build.

        public int MyInteger
        {
            get
            {
                IProp<int> prop = GetTypedProp<int>(nameof(MyInteger));
                return prop.TypedValue;
            }
            set
            {
                IProp<int> prop = GetTypedProp<int>(nameof(MyInteger));
                prop.TypedValue = value;
            }
        }

        public string MyString
        {
            get
            {
                return GetIt<string>(nameof(MyString));

            }
            set
            {
                SetIt<string>(value, nameof(MyString));
            }
        }

        #endregion

    }
}
