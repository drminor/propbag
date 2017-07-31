using System;
using System.Collections.Generic;
using DRM.Ipnwv;
using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class LooseModel : PropBag
    {
        public LooseModel() : this(PropBagTypeSafetyMode.OnlyTypedAccess) { }

        public LooseModel(PropBagTypeSafetyMode typeSafetyMode)
            : base(typeSafetyMode)
        {
            //AddProp<object>("PropObject");
            //AddProp<string>("PropString");
            //AddProp<bool>("PropBool", false);
            //AddProp<int>("PropInt");
            //AddProp<TimeSpan>("PropTimeSpan");
            //AddProp<Uri>("PropUri");
            //AddProp<Lazy<int>>("PropLazyInt");
        }

        #region Property Declarations

        public object PropObject
        {
            get
            {
                return (object)this["PropObject"];
            }
            set
            {
                this["PropObject"] = value;
            }
        }

        public string PropString
        {
            get
            {
                return (string)this["PropString"];
            }
            set
            {
                this["PropString"] = value;
            }
        }

        public bool PropBool
        {
            get
            {
                return (bool)this["PropBool"];
            }
            set
            {
                this["PropBool"] = value;
            }
        }

        public int PropInt
        {
            get
            {
                return GetIt<int>();
            }
            set
            {
                SetIt<int>(value);
            }
        }

        public TimeSpan PropTimeSpan
        {
            get
            {
                return GetIt<TimeSpan>();
            }
            set
            {
                SetIt<TimeSpan>(value);
            }
        }

        public Uri PropUri
        {
            get
            {
                return GetIt<Uri>();
            }
            set
            {
                SetIt<Uri>(value);
            }
        }

        public Lazy<int> PropLazyInt
        {
            get
            {
                return GetIt<Lazy<int>>();
            }
            set
            {
                SetIt<Lazy<int>>(value);
            }
        }

        #endregion

        #region PropetyChangedWithTVals Event Declarations

        public event PropertyChangedWithTValsHandler<object> PropObjectChanged
        {
            add
            {
                AddToPropChanged<object>(value);
            }
            remove
            {
                RemoveFromPropChanged<object>(value);
            }
        }

        public event PropertyChangedWithTValsHandler<string> PropStringChanged
        {
            add
            {
                AddToPropChanged<string>(value);
            }
            remove
            {
                RemoveFromPropChanged<string>(value);
            }
        }

        public event PropertyChangedWithTValsHandler<bool> PropBoolChanged
        {
            add
            {
                AddToPropChanged<bool>(value);
            }
            remove
            {
                RemoveFromPropChanged<bool>(value);
            }
        }

        public event PropertyChangedWithTValsHandler<int> PropIntChanged
        {
            add
            {
                AddToPropChanged<int>(value);
            }
            remove
            {
                RemoveFromPropChanged<int>(value);
            }
        }

        public event PropertyChangedWithTValsHandler<TimeSpan> PropTimeSpanChanged
        {
            add
            {
                AddToPropChanged<TimeSpan>(value);
            }
            remove
            {
                RemoveFromPropChanged<TimeSpan>(value);
            }
        }

        public event PropertyChangedWithTValsHandler<Uri> PropUriChanged
        {
            add
            {
                AddToPropChanged<Uri>(value);
            }
            remove
            {
                RemoveFromPropChanged<Uri>(value);
            }
        }

        public event PropertyChangedWithTValsHandler<Lazy<int>> PropLazyIntChanged
        {
            add
            {
                AddToPropChanged<Lazy<int>>(value);
            }
            remove
            {
                RemoveFromPropChanged<Lazy<int>>(value);
            }
        }

        #endregion

    }
}
