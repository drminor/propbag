﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;
    #endregion

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
    public interface IProp<T> : IProp //, INotifyPCTyped<T> //, IProvideATypedEventManager<T>
    {
        T TypedValue { get; set; }

        bool CompareTo(T value);
        bool Compare(T val1, T val2);

        void DoWhenChanged(T oldVal, T newVal);

        //void RaiseEvents(IEnumerable<ISubscriptionGen> typedSubs);
        //void RaiseEventsForParent(IEnumerable<ISubscriptionGen> typedSubs, object parent, string propertyName, object oldVal, object newVal);

        Attribute[] Attributes { get; }
    }

    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// </summary>
    public interface IProp : INotifyPCObject
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

        void OnPropertyChangedWithObjectVals(PropNameType propertyName, object oldVal, object newVal);

        void CleanUpTyped();
    }

    /// <summary>
    /// Allows access from code in the TypeSafePropertyBag assembly, but not from the PropBag assembly.
    /// </summary>
    internal interface IPropDataInternal : IPropData
    {
        CompositeKeyType CKey { get; }
        bool IsPropBag { get; }

        //// The ObjectId assigned to the value of this Prop, if the TypedProp.Type is, or derives from, IPropBag.
        //ObjectIdType ChildObjectId { get; set; }

        // On those occasions when the IProp starts off with Type = object, and then later, the type is firmed up,
        // The IPropBag needs to be able to have a new IProp created with the correct type
        // and that new IProp needs to replace the original IProp.
        void SetTypedProp(IProp value);
    }

    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that inherit from AbstractProp.
    /// </summary>
    public interface IPropData : INotifyPCObject
    {
        PropIdType PropId { get; }

        bool IsEmpty { get; }

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; }

        ValPlusType ValuePlusType();

        void CleanUp(bool doTypedCleanup);

        void OnPropertyChangedWithObjectVals(PropNameType propertyName, object oldVal, object newVal);
    }
}