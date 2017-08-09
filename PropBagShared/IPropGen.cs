using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    /// <summary>
    /// All Properties share these features.
    /// </summary>
    public interface IPropGen
    {
        Type Type { get; }
        bool TypeIsSolid { get;}
        bool HasStore { get; }

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; set; } 

        // Property Changed with typed values support
        event PropertyChangedWithValsHandler PropertyChangedWithVals;
        void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal);

        void SubscribeToPropChanged(Action<object, object> doOnChange);
        bool UnSubscribeToPropChanged(Action<object, object> doOnChange);

        object Value { get; }

        void CleanUp();

    }
}
