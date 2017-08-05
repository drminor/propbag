using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Ipnwvc;
using DRM.PropBag;

using ExtValProviderExample;

namespace PropBagLib.Tests
{
    public partial class ExtStoreModel 
    {

        static public ExtStoreModel Create(AbstractPropFactory factory)
        {

            ExtStoreModel esm = new ExtStoreModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered, factory);

            PropExternStore<int> pi = (PropExternStore<int>) esm.AddProp<int>("PropInt", null, false, null);
            PropExternStore<string> ps = (PropExternStore<string>) esm.AddProp<string>("PropString", null, false, null);

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
                return GetIt<int>();
            }
            set
            {
                SetIt<int>(value);
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

    }
}
