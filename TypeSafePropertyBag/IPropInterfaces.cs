using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Extends the IProp<typeparamref name="T"/> interface with features
    /// that are only avaialble within the PubPropBag assembly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPropPrivate<T> : IProp<T>
    {
        bool DoAfterNotify { get; set; }
        bool UpdateDoWhenChangedAction(Action<T, T> doWHenChangedAction, bool? doAfterNotify);
    }

    /// <summary>
    /// All properties have these features based on the type of the property.
    /// Objects that implement this interface are often created by an instance of a class that inherits from AbstractPropFactory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProp<T> : IPropGen, IProp
    {
        T TypedValue { get; set; }

        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        void DoWhenChanged(T oldVal, T newVal);

        // Property Changed with typed values support
        event PropertyChangedWithTValsHandler<T> PropertyChangedWithTVals;
        void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal);

        void SubscribeToPropChanged(Action<T, T> doOnChange);
        bool UnSubscribeToPropChanged(Action<T, T> doOnChange);
    }

    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that inherit from AbstractProp.
    /// </summary>
    public interface IPropGen
    {
        Type Type { get; }
        bool TypeIsSolid { get; }
        bool HasStore { get; }

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; }

        // Property Changed with typed values support
        event PropertyChangedWithValsHandler PropertyChangedWithVals;
        void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal);

        void SubscribeToPropChanged(Action<object, object> doOnChange);
        bool UnSubscribeToPropChanged(Action<object, object> doOnChange);

        object Value { get; }

        ValPlusType ValuePlusType();

        void CleanUp();

    }

    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// These features are often implemented by an instance of a class that inherits from AbstractPropFactory.
    /// </summary>
    public interface IProp
    {
        object TypedValueAsObject { get; }
        bool ValueIsDefined { get; }

        /// <summary>
        /// Marks the property as having an undefined value.
        /// </summary>
        /// <returns>True, if the value was defined at the time this call was made.</returns>
        bool SetValueToUndefined();

        bool CallBacksHappenAfterPubEvents { get; }
        bool HasCallBack { get; }
        bool HasChangedWithTValSubscribers { get; }

        void CleanUpTyped();
    }
}
