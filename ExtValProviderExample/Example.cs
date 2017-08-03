using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtValProviderExample
{
    public class Example
    {

        Dictionary<Guid, object> _propVals;

        public Example()
        {
            _propVals = new Dictionary<Guid, object>();
        }

        object this[Guid key]
        {
            get {return _propVals[key]; }
            set {_propVals[key] = value; }
        }



    }

}
