using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag.Fundamentals;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.EventManagement
{
    public class AbstractPCEventManager<ExKeyT, CompT, L1T, L2T, L2TRaw, PropDataT> where PropDataT : IPropGen where ExKeyT : IExplodedKey<CompT, L1T, L2T>
    {
        private IL2KeyMan<uint, string> _level2KeyManager;
        private ICKeyMan<SimpleExKey, ulong, uint, uint, string> _compKeyManager;
        private readonly IObjectIdDictionary<ExKeyT, CompT, L1T, L2T, L2TRaw, PropDataT> _propertyStore;

        public AbstractPCEventManager(IL2KeyMan<uint, string> level2KeyManager, ICKeyMan<SimpleExKey, ulong, uint, uint, string> compKeyManager, IObjectIdDictionary<ExKeyT, CompT, L1T, L2T, L2TRaw, PropDataT> propertyStore)
        {
            _propertyStore = propertyStore;
            _level2KeyManager = level2KeyManager;
            _compKeyManager = compKeyManager;
        }

        public PropDataT GetVal(ulong cKey, object newVal)
        {
            return default(PropDataT);
        }
    }
}
