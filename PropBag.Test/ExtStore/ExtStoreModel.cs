using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;

namespace PropBagLib.Tests
{
    public partial class ExtStoreModel 
    {

        static public ExtStoreModel Create(IPropFactory factory)
        {

            ExtStoreModel esm = new ExtStoreModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered, factory);

            PropExternStore<int> pi = (PropExternStore<int>)esm.AddPropNoStore<int>("PropInt", null, false, null);
            PropExternStore<string> ps = (PropExternStore<string>)esm.AddPropNoStore<string>("PropString", null, false, null);

            ExtData ed = new ExtData();

            pi.Getter = (x) => ed.PropIntStandard;
            pi.Setter = (x, v) => ed.PropIntStandard = v;

            ps.Getter = (x) => ed.PropStringStandard;
            ps.Setter = (x, v) => ed.PropStringStandard = v;

            return esm;
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

        public string PropString
        {
            get
            {
                return GetIt<string>(nameof(PropString));
            }
            set
            {
                SetIt<string>(value: value, propertyName: nameof(PropString));
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

    }
}
