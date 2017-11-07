using DRM.TypeSafePropertyBag.EventManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace DRM.TypeSafePropertyBag
{
    // Typically implemented by TypedTableBase<T> Class
    public interface IDTPropPrivate<CT,T> : ICPropPrivate<CT,T> where CT: IEnumerable<T>
    {
        DataTable DataTable { get; }
    }

    // Typically implemented by TypedTableBase<T> Class
    public interface IDTProp<CT,T> : ICProp<CT,T> where CT: IEnumerable<T>
    {
        DataTable ReadOnlyDataTable { get; }
    }

    public interface ICPropPrivate<CT,T> : ICProp<CT,T>, IPropPrivate<CT> where CT: IEnumerable<T>
    {
        ObservableCollection<T> ObservableCollection { get; }
        void SetListSource(IListSource value);
    }

    public interface ICProp<CT,T> : IProp<CT> where CT: IEnumerable<T>
    {
        ReadOnlyObservableCollection<T> ReadOnlyObservableCollection { get; }
    }

    /// <summary>
    /// Extends the IProp<typeparamref name="T"/> interface with features
    /// that are only avaialble within the PubPropBag assembly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPropPrivate<T> : IProp<T>
    {
        bool DoAfterNotify { get; set; }
        Action<T, T> DoWHenChangedAction { get; }

        bool UpdateDoWhenChangedAction(Action<T, T> doWhenChangedAction, bool? doAfterNotify);
    }

    /// <summary>
    /// All properties have these features based on the type of the property.
    /// Objects that implement this interface are often created by an instance of a class that inherits from AbstractPropFactory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProp<T> : IProp, INotifyPCTyped<T>, IProvideATypedEventManager<T>
    {
        T TypedValue { get; set; }

        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        void DoWhenChanged(T oldVal, T newVal);

        // Property Changed with typed values support
        //event EventHandler<PropertyChangedWithTValsEventArgs<T>> PropertyChangedWithTVals;
        void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal);

        // TODO: Consider moving these to the IPropPrivate<T> interface.
        // and using the WeakEventManager style.
        //void SubscribeToPropChanged(Action<T, T> doOnChange);
        //bool UnSubscribeToPropChanged(Action<T, T> doOnChange);

        Attribute[] Attributes { get; }
    }

    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that inherit from AbstractProp.
    /// </summary>
    public interface IPropGen 
    {
        ulong PropId { get; }
        bool IsEmpty { get; }

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; }

        object Value { get; }

        ValPlusType ValuePlusType();

        void CleanUp(bool doTypedCleanup);
    }

    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// </summary>
    public interface IProp : INotifyPCGen
    {
        PropKindEnum PropKind { get; }
        Type Type { get; }
        bool TypeIsSolid { get; }
        bool HasStore { get; }

        IListSource ListSource { get; }

        object TypedValueAsObject { get; }
        ValPlusType GetValuePlusType();

        bool ValueIsDefined { get; }

        /// <summary>
        /// Marks the property as having an undefined value.
        /// </summary>
        /// <returns>True, if the value was defined at the time this call was made.</returns>
        bool SetValueToUndefined();


        // Property Changed with typed values support
        //event EventHandler<PropertyChangedWithValsEventArgs> PropertyChangedWithVals;
        void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal);

        //void SubscribeToPropChanged(Action<object, object> doOnChange);
        //bool UnSubscribeToPropChanged(Action<object, object> doOnChange);


        bool CallBacksHappenAfterPubEvents { get; }
        bool HasCallBack { get; }
        bool HasChangedWithTValSubscribers { get; }

        void CleanUpTyped();
    }
}
