using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.TypeSafePropertyBag
{
    public class TypeSafePropBagMetaData
    {
        IPropFactory ThePropFactory { get; }

        public PropBagTypeSafetyMode TypeSafetyMode { get; protected set; }

        /// <summary>
        /// If true, attempting to set a property for which no call to AddProp has been made, will cause an exception to thrown.
        /// </summary>
        public bool AllPropsMustBeRegistered { get; }

        /// <summary>
        /// If not true, attempting to set a property, not previously set with a call to AddProp or SetIt<typeparamref name="T"/>, will cause an exception to be thrown.
        /// </summary>
        public bool OnlyTypedAccess { get; }

        public ReadMissingPropPolicyEnum ReadMissingPropPolicy { get; }

        public bool ReturnDefaultForUndefined { get; }

        public string ClassName { get; }

        public string ClassFullName { get; }

    }
}
