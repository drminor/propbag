using System;
using System.Collections.Generic;
using DRM.Ipnwvc;
using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class OnlyTypedAccessModel : PropBag
    {
        public OnlyTypedAccessModel() : this(PropBagTypeSafetyMode.OnlyTypedAccess) { }

        public OnlyTypedAccessModel(PropBagTypeSafetyMode typeSafetyMode)
            : base(typeSafetyMode)
        {
            AddProp<object>("PropObject", null, false, null);
            AddProp<string>("PropString", DoWhenStringChanged, false, null);
            AddProp<string>("PropStringCallDoAfter", DoWhenStringChanged, true, null);
            AddPropObjComp<string>("PropStringUseRefComp", DoWhenStringChanged, false, null);
            AddProp<bool>("PropBool", null, false, null, false);
            AddProp<int>("PropInt", null, false, null);
            AddProp<TimeSpan>("PropTimeSpan", null, false, null);
            AddProp<Uri>("PropUri", null, false, null);
            AddProp<Lazy<int>>("PropLazyInt", null, false, null);
            //AddProp<Nullable<int>>("PropNullableInt", DoWhenNullIntChanged, false, null, -1);
            //AddProp<ICollection<int>>("PropICollectionInt", DoWhenICollectionIntChanged), false, null);

        }

        #region Property Declarations

        public object PropObject
        {
            get
            {
                return GetIt<object>();
            }
            set
            {
                SetIt<object>(value);
            }
        }

        public string PropString
        {
            get
            {
                return GetIt<string>();
            }
            set
            {
                SetIt<string>(value);
            }
        }

        public string PropStringCallDoAfter
        {
            get
            {
                return GetIt<string>();
            }
            set
            {
                SetIt<string>(value);
            }
        }
        public bool PropBool
        {
            get
            {
                return GetIt<bool>();
            }
            set
            {
                SetIt<bool>(value);
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

        public event PropertyChangedWithTValsHandler<string> PropStringCallDoAfterChanged
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
