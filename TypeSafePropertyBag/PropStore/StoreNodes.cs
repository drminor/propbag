using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeSafePropertyBag.PropStore
{
    #region Type Aliases 
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    #endregion

    public class StoreNodes
    {
        readonly Dictionary<ExKeyT, StoreNodeBag> _store;

        public StoreNodes()
        {
            _store = new Dictionary<ExKeyT, StoreNodeBag>();
        }

        StoreNodeBag Add(ExKeyT cKey, IPropBagProxy propBagProxy)
        {
            StoreNodeBag newBag = new StoreNodeBag(cKey, propBagProxy);
            _store.Add(cKey, newBag);
            return newBag;
        }


    }
}
