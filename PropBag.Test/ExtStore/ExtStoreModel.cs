using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.PropBag;
using DRM.TypeSafePropertyBag;

namespace PropBagLib.Tests
{
    public partial class ExtStoreModel 
    {

        static public ExtStoreModel Create(AbstractPropFactory factory)
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
                SetIt<int>(value: value);
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
                SetIt<string>(value: value);
            }
        }


        public event PropertyChangedWithTValsHandler<int> PropIntChanged
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

        public event PropertyChangedWithTValsHandler<string> PropStringChanged
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
