
namespace DRM.TypeSafePropertyBag
{
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