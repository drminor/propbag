namespace DRM.TypeSafePropertyBag
{
    public enum PropBagTypeSafetyMode
    {
        /// <summary>
        /// Neither AllPropsMustBeRegistered, nor OnlyTypedAccess. Attempting to get a property that 
        // has not been Registered will throw an exception.
        // Reading a property whose value is set to undefined will return the default value.
        // Setting a property, not yet registered, will register the property.
        /// </summary>
        None=0,

        /// <summary>
        /// Same as none, except exceptions are thrown when attempting to get the value of a property 
        /// whose value has been set to undefined.
        /// </summary>
        HonorUndefined=1,

        /// <summary>
        /// Same as None, properties not yet created will be created on get.
        /// </summary>
        RegisterOnGetLoose=2,

        /// <summary>
        /// All access must provide a non-null type with the value being access (for both get and set operations.)
        /// Accessing properties with a value of undefined will throw an exception.
        /// </summary>
        OnlyTypedAccess=3,

        /// <summary>
        /// Same as OnlyTypedAccess, but getting a property not yet created, will create it.
        /// </summary>
        RegisterOnGetSafe=4,

        /// <summary>
        /// All access must be to registered properties.
        /// </summary>
        AllPropsMustBeRegistered=5,

        /// <summary>
        /// AllPropsMustBeRegistered + OnlyTypedAccess.
        /// </summary>
        Tight=6,

        /// <summary>
        /// AllPropsMustBeRegistered + OnlyTypedAccess. TryGetOperations fail if property does not exist, PropertyExist always throws an exception.
        /// </summary>
        Locked=7
    }




}
