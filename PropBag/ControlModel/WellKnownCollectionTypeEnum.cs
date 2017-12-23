using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlModel
{
    public enum WellKnownCollectionTypeEnum
    {
        Enumerable,
        TypedEnumerable,
        ObservableCollection,   // IObsCollection<T>
        ObservableCollectionFB  // ObservableCollection<T>
    }
}
