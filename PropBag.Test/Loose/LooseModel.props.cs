using System;
using DRM.TypeSafePropertyBag;
using DRM.PropBag; 

namespace PropBagLib.Tests
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public partial class LooseModel : PropBag
    {

		public LooseModel(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName: null)
        {
            AddProp<object>("PropObject");
            AddProp<string>("PropString");
            AddProp<string>("PropString2", null, null, "12");
            AddProp<bool>("PropBool", null, initialValue: false);
            AddProp<int>("PropInt");
            AddProp<TimeSpan>("PropTimeSpan");
            AddProp<Uri>("PropUri");
            AddProp<Lazy<int>>("PropLazyInt");

            //AddBinding<string>("PropString2", "PropString", FF);

        }

        public void FF(string s1, string s2)
        {
            System.Diagnostics.Debug.WriteLine($"The strings are: {s1} + {s2}");
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

        public event EventHandler<PcTypedEventArgs<object>> PropObjectChanged
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

        public event EventHandler<PcTypedEventArgs<string>> PropStringChanged
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

        public event EventHandler<PcTypedEventArgs<bool>> PropBoolChanged
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

        public event EventHandler<PcTypedEventArgs<int>> PropIntChanged
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

        public event EventHandler<PcTypedEventArgs<TimeSpan>> PropTimeSpanChanged
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

        public event EventHandler<PcTypedEventArgs<Uri>> PropUriChanged
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

        public event EventHandler<PcTypedEventArgs<Lazy<int>>> PropLazyIntChanged
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
