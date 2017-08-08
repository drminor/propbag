using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    public interface IPropGen
    {
        Type Type { get; }
        bool TypeIsSolid { get;}
        bool HasStore { get; }

        // Property Changed with typed values support
        event PropertyChangedWithValsHandler PropertyChangedWithVals;
        void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal);

        void SubscribeToPropChanged(Action<object, object> doOnChange);
        bool UnSubscribeToPropChanged(Action<object, object> doOnChange);

        object Value { get; }

    }
}
