using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.PropBag.Collections
{
    public class PbListSource : PbAbstractListSource, IListSource
    {
        // This uses the exact implementation provided by the abstract base class: PbAstractListSource -- so far.
        public PbListSource(Func<object, IList> getter, object component) : base(getter, component) 
        {
        }

    }
}
