using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using DRM.Inpcwv;

namespace DRM.PropBag
{
    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that inherit from AbstractProp.
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

        ValPlusType ValuePlusType();

        void CleanUp();

    }
}
