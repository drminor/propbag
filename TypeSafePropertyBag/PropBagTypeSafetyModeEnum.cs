namespace DRM.TypeSafePropertyBag
{
    public enum PropBagTypeSafetyMode
    {
        ///// <summary>
        ///// Same as loose, but no exceptions will be thrown on get, instead the default value will be 
        ///// returned. If a default value cannot be created an exception will be thrown.
        ///// Access to properties that have an undefined value will return the default value.
        ///// </summary>
        //None = 0,

        /// <summary>
        /// Neither AllPropsMustBeRegistered, nor OnlyTypedAccess. Attempting to get a property that 
        // has not been Registered will throw an exception.
        // Reading a property whose value is set to undefined will return the default value.
        // Setting a property, not yet registered, will register the property.
        /// </summary>
        None = 0,

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
        /// Accessing properties with a value of undefined will throw an exception.
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
        Tight,

        /// <summary>
        /// AllPropsMustBeRegistered + OnlyTypedAccess. TryGetOperations fail if property does not exist, PropertyExist always throws an exception.
        /// </summary>
        Locked
    }

    // TODO: Consider creating a WriteMissingPropPolicyEnum.
    // TODO: Consider handling "Allow Reflection" separately and removing TypeSafetyMode: "Locked."
    public enum ReadMissingPropPolicyEnum
    {
        /// <summary>
        /// Reading from a missing property, including TryGetValue, TryGetPropGen, TryGetTypeOfProperty
        /// will throw an InvalidOperationException. Calling PropertyExists will always throw and exception.
        /// Used for TypeSafetyModes: Locked.
        /// </summary>
        NotAllowed,

        /// <summary>
        /// Reading the value from a missing property, including GetTypeOfProperty will throw an InvalidOperationException.
        /// TryGetValue, TryGetTypeOfProperty and PropertyExists will return false.
        /// TryGetPropGen will return null.

        /// Used for TypeSafetyModes: Tight, AllPropsMustBeRegistered, OnlyTypedAccess, None, and HonorUndefined.
        /// </summary>
        Allowed,

        /// <summary>
        /// Reading a missing property will cause that property to be created with it's default value.
        /// TryGetValue, TryGetTypeOfProperty and PropertyExists will return false.
        /// GetTypeOfProperty and TryGetPropGen will return null.
        /// 
        /// Used for TypeSafetyModes: RegisterOnGetLoose and RegisterOnGetSafe.
        /// </summary>
        Register
    }


}
