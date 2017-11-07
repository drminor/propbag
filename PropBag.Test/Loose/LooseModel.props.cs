using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag;

namespace PropBagLib.Tests
{
    public partial class LooseModel : PropBag
    {

        public LooseModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered) { }

		public LooseModel(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
        {
            AddProp<object>("PropObject");
            AddProp<string>("PropString");
            AddProp<bool>("PropBool", null, false, null, initialValue: false);
            AddProp<int>("PropInt");
            AddProp<TimeSpan>("PropTimeSpan");
            AddProp<Uri>("PropUri");
            AddProp<Lazy<int>>("PropLazyInt");
        }

        #region Property Declarations

        public object PropObject
        {
            get
            {
                return (object)this["System.Object", "PropObject"];
            }
            set
            {
                this["System.Object", "PropObject"] = value;
            }
        }

        public string PropString
        {
            get
            {
                return (string)this["System.String", "PropString"];
            }
            set
            {
                this["System.String", "PropString"] = value;
            }
        }

        public bool PropBool
        {
            get
            {
                return (bool)this["System.Boolean","PropBool"];
            }
            set
            {
                this["System.Boolean", "PropBool"] = value;
            }
        }

        public int PropInt
        {
            get
            {
                return GetIt<int>(nameof(PropInt));
            }
            set
            {
                SetIt<int>(value: value, propertyName: nameof(PropInt));
            }
        }

        public TimeSpan PropTimeSpan
        {
            get
            {
                return GetIt<TimeSpan>(nameof(PropTimeSpan));
            }
            set
            {
                SetIt<TimeSpan>(value: value, propertyName: nameof(PropTimeSpan));
            }
        }

        public Uri PropUri
        {
            get
            {
                return GetIt<Uri>(nameof(PropUri));
            }
            set
            {
                SetIt<Uri>(value: value, propertyName: nameof(PropUri));
            }
        }

        public Lazy<int> PropLazyInt
        {
            get
            {
                return GetIt<Lazy<int>>(nameof(PropLazyInt));
            }
            set
            {
                SetIt<Lazy<int>>(value: value, propertyName: nameof(PropLazyInt));
            }
        }

        #endregion

        #region PropetyChangedWithTVals Event Declarations

        public event EventHandler<PCTypedEventArgs<object>> PropObjectChanged
        {
            add
            {
                AddToPropChanged<object>(value, nameof(PropObjectChanged));
            }
            remove
            {
                RemoveFromPropChanged<object>(value,nameof(PropObjectChanged));
            }
        }

        public event EventHandler<PCTypedEventArgs<string>> PropStringChanged
        {
            add
            {
                AddToPropChanged<string>(value, nameof(PropStringChanged));
            }
            remove
            {
                RemoveFromPropChanged<string>(value, nameof(PropStringChanged));
            }
        }

        public event EventHandler<PCTypedEventArgs<bool>> PropBoolChanged
        {
            add
            {
                AddToPropChanged<bool>(value, nameof(PropBoolChanged));
            }
            remove
            {
                RemoveFromPropChanged<bool>(value, nameof(PropBoolChanged));
            }
        }

        public event EventHandler<PCTypedEventArgs<int>> PropIntChanged
        {
            add
            {
                AddToPropChanged<int>(value, nameof(PropIntChanged));
            }
            remove
            {
                RemoveFromPropChanged<int>(value, nameof(PropIntChanged));
            }
        }

        public event EventHandler<PCTypedEventArgs<TimeSpan>> PropTimeSpanChanged
        {
            add
            {
                AddToPropChanged<TimeSpan>(value, nameof(PropTimeSpanChanged));
            }
            remove
            {
                RemoveFromPropChanged<TimeSpan>(value, nameof(PropTimeSpanChanged));
            }
        }

        public event EventHandler<PCTypedEventArgs<Uri>> PropUriChanged
        {
            add
            {
                AddToPropChanged<Uri>(value, nameof(PropUriChanged));
            }
            remove
            {
                RemoveFromPropChanged<Uri>(value, nameof(PropUriChanged));
            }
        }

        public event EventHandler<PCTypedEventArgs<Lazy<int>>> PropLazyIntChanged
        {
            add
            {
                AddToPropChanged<Lazy<int>>(value, nameof(PropLazyIntChanged));
            }
            remove
            {
                RemoveFromPropChanged<Lazy<int>>(value, nameof(PropLazyIntChanged));
            }
        }

        #endregion

    }
}
