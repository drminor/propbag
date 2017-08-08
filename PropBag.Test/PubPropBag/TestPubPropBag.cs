using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Ipnwvc;
using DRM.PropBag;

namespace PropBagLib.Tests
{
    public class TestPubPropBag : PubPropBag
    {

        public TestPubPropBag() : base(PropBagTypeSafetyMode.AllPropsMustBeRegistered) 
        {
            
        }

    }
}
