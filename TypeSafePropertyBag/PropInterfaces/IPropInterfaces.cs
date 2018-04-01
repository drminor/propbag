using System;

namespace DRM.TypeSafePropertyBag
{
    using IRegisterBindingsProxyType = IRegisterBindingsProxy<UInt32>;
    using PropNameType = String;

    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that implement IProp.)
    /// </summary>
    public interface IPropData
    {
        bool IsEmpty { get; }
        bool IsPropBag { get; }
        //bool IsPrivate { get; } // TODO: Consider adding the ability to register private PropItems.

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; }
        DoSetDelegate DoSetDelegate { get; set; }
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

    public interface IPropTemplate : IEquatable<IPropTemplate>
    {
        PropKindEnum PropKind { get; }
        Type Type { get; }
        bool IsPropBag { get; }
        PropStorageStrategyEnum StorageStrategy { get; }

        Attribute[] Attributes { get; }

        Type PropFactoryType { get; }
        bool ComparerIsDefault { get; }
        bool ComparerIsRefEquality { get; }
        Delegate ComparerProxy { get; }

        bool DefaultValFuncIsDefault { get; }
        object GetDefaultValFuncProxy { get; }

        DoSetDelegate DoSetDelegate { get; set; }

        // TODO: The PropCreator property is just being used to carry the value temporarily.
        // We need to have the PropFactory return this in a separate, dedicated, out field in 
        // each of the Create<T>, etc. methods.
        // Reason: We want the IPropTemplate to be as generic as possible. If we
        // include the PropCreator value when comparing for equality we will have one for each PropItem.
        Func<PropNameType, object, bool, IPropTemplate, IProp> PropCreator { get; set; }
    }

    public interface IPropTemplate<T> : IPropTemplate
    {
        Func<T, T, bool> Comparer { get; }
        Func<string, T> GetDefaultVal { get; }
    }

    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// </summary>
    public interface IProp : IRegisterBindingsProxyType, ICloneable
    {
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

        IPropTemplate PropTemplate { get; }
    }

    /// <summary>
    /// All properties have these features based on the type of the property.
    /// Objects that implement this interface are often created by an instance of a class that inherits from AbstractPropFactory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProp<T> : IProp 
    {
        T TypedValue { get; set; }

        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        IPropTemplate<T> TypedPropTemplate { get; }
    }
}
