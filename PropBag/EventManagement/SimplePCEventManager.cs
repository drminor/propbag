using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.EventManagement
{
    public class SimplePCEventManager<PropDataT> : AbstractPCEventManager<ulong, uint, uint, string, PropDataT> where PropDataT : IPropGen
    {
        public SimplePCEventManager
            (
            IObjectIdDictionary<ulong, uint, uint, string, PropDataT> propertyStore,
            ICKeyMan<ulong, uint, uint, string> compKeyManager,
            IL2KeyMan<uint, string> level2KeyManager
            )
            : base(level2KeyManager, compKeyManager, propertyStore)
        {
        }


    }
}
