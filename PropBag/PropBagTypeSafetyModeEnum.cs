using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public enum PropBagTypeSafetyMode
    {
        /// <summary>
        /// Same as loose, but no exceptions will be thrown on get, instead the default value will be 
        /// returned. If a default value cannot be created an exception will be thrown.
        /// Access to properties that have an undefined value will return the default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Neither AllPropsMustBeRegistered, nor OnlyTypedAccess. Attempting to get a property that 
        // has not been Registered, Registered with a non-defined value, or not Set previously will throw an exception.
        /// </summary>
        Loose,

        /// <summary>
        /// Same as none, except exceptions are thrown when attempting to get the value of a property 
        /// whose value has been set to undefined.
        /// </summary>
        HonorUndefined,

        /// <summary>
        /// Same as None, properties not yet created will be created on get.
        /// </summary>
        RegisterOnGetLoose,

        /// <summary>
        /// All access must provide a non-null type with the value being access (for both get and set operations.)
        /// Accessing properties with a value of of undefined will throw an exception.
        /// </summary>
        OnlyTypedAccess,

        /// <summary>
        /// Same as OnlyTypedAccess, but getting a property not yet created, will create it.
        /// </summary>
        RegisterOnGetSafe,

        /// <summary>
        /// All access must be to registered properties.
        /// </summary>
        AllPropsMustBeRegistered,

        /// <summary>
        /// AllPropsMustBeRegistered + OnlyTypedAccess.
        /// </summary>
        Tight
    }

    public enum ReadMissingPropPolicyEnum
    {
        NotAllowed,
        Allowed,
        Register
    }


}
