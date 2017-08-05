using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Ipnwvc;
using DRM.PropBag;

using ExtValProviderExample;

namespace PropBagLib.Tests.ExtStoreTests
{
    public partial class ExtStoreModel 
    {

        Example extPropStore = new Example();

        public ExtStoreModel()
        {
            
        }

        public object ExtValG(Guid key)
        {
            return null;
        }

        public void ExtValS(Guid key, object value)
        {

        }


    }
}
