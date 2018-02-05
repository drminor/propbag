using System;

namespace DRM.TypeSafePropertyBag
{
    using IRegisterBindingsProxyType = IRegisterBindingsProxy<UInt32>;
    using PropNameType = String;

    /// <summary>
    /// All properties have these features based on the type of the property.
    /// Objects that implement this interface are often created by an instance of a class that inherits from AbstractPropFactory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProp<T> : IPropTemplate<T>, IProp 
    {
        T TypedValue { get; set; }

        bool CompareTo(T value);
        bool Compare(T val1, T val2);
    }

    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// </summary>
    public interface IProp : IPropTemplate, IRegisterBindingsProxyType, ICloneable
    {
        // Unique to each instance.
        PropNameType PropertyName { get; }
        bool TypeIsSolid { get; }
        bool ValueIsDefined { get; }

        event EventHandler<EventArgs> ValueChanged; // Used by virtual properties.

        object TypedValueAsObject { get; }
        ValPlusType GetValuePlusType();

        bool ReturnDefaultForUndefined { get; }

        /// <summary>
        /// Marks the property as having an undefined value.
        /// </summary>
        /// <returns>True, if the value was defined at the time this call was made.</returns>
        bool SetValueToUndefined();

        void CleanUpTyped();
    }

    public interface IPropTemplate<T> : IPropTemplate
    {
        Func<T, T, bool> Comparer { get; }
        Func<string, T> GetDefaultValFunc { get; }
    }

    public interface IPropTemplate
    {
        PropKindEnum PropKind { get; }
        Type Type { get; }
        PropStorageStrategyEnum StorageStrategy { get; }

        Attribute[] Attributes { get; }

        object ComparerProxy { get; }
        object GetDefaultValFuncProxy { get; }
    }

    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that implement IProp (derive from PropBase.)
    /// </summary>
    public interface IPropData
    {
        //PropIdType PropId { get; }

        bool IsEmpty { get; }
        bool IsPropBag { get; }
        //bool IsPrivate { get; } // TODO: Consider adding the ability to register private PropItems.

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; }
    }

    /// <summary>
    /// This interface is used by the PropStore / PropStoreAccessService
    /// </summary>
    internal interface IPropDataInternal : IPropData
    {
        // On those occasions when the IProp starts off with Type = object, and then later, the type is firmed up,
        // The IPropBag needs to be able to have a new IProp created with the correct type
        // and that new IProp needs to replace the original IProp.
        void SetTypedProp(PropNameType propertyName, IProp value);

        void CleanUp(bool doTypedCleanup);
    }
}
