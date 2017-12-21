﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PropIdType = UInt32;
    using PropNameType = String;

    // Typically implemented by TypedTableBase<T> Class
    public interface IDTPropPrivate : IProp
    {
        DataTable DataTable { get; }
    }

    // Typically implemented by TypedTableBase<T> Class
    public interface IDTProp : IProp
    {
        DataTable ReadOnlyDataTable { get; }
    }

    // Collection View Source
    public interface ICViewProp<TCVS, T> : IReadOnlyCViewProp<TCVS, T> where TCVS: class
    {
        object Source { get; set; }
    }

    // CollectionViewSource -- ReadOnly 
    public interface IReadOnlyCViewProp<TCVS, T> : IProp<TCVS> where TCVS : class
    {
        ICollectionView View { get; }
        ICollectionView this[string key] { get; }
        IReadOnlyObsCollection<T> GetReadOnlyObservableCollection();
    }

    // ObsCollection<T> interface
    public interface ICProp<CT,T> : IETypedProp<CT, T>, IReadOnlyCProp<CT, T>/*, IListSourceProvider<CT,T>*/ where CT : IObsCollection<T>
    {

    }

    // ObsCollection<T> interface -- ReadOnly
    public interface IReadOnlyCProp<CT,T> : IReadOnlyETypedProp<CT, T>/*, IListSource*/ where CT : IReadOnlyObsCollection<T>
    {

    }



    // ObservableCollection<T>
    public interface ICPropFB<CT, T> : IProp<CT>/*, IListSourceProvider<CT,T>*/ where CT : ObservableCollection<T>
    {
        ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection();
    }

    // ObservableCollection<T> -- ReadOnly
    public interface IReadOnlyCPropFB<CT, T> : IProp<CT>/*, IListSource*/ where CT : ReadOnlyObservableCollection<T>
    {

    }


    



    // IEnumerable<T> Collections
    public interface IETypedProp<CT, T> : IEProp<CT>, IReadOnlyETypedProp<CT, T>/*, IListSourceProvider<CT,T>*/ where CT : IEnumerable<T>
    {

    }

    // IEnumerable<T> Collections -- ReadOnly
    public interface IReadOnlyETypedProp<CT, T> : IReadOnlyEProp<CT>/*, IListSource*/ where CT : IEnumerable<T>
    {

    }

    // IEnumerable Collections
    public interface IEProp<CT> : IProp<CT>, IReadOnlyEProp<CT> where CT : IEnumerable
    {
        //void SetListSource(IListSource value);
    }

    // IEnumerable Collections -- ReadOnly
    public interface IReadOnlyEProp<CT> : IProp<CT> where CT : IEnumerable
    {
        //IList List { get; }
    }

    ///// <summary>
    ///// Extends the IProp<typeparamref name="T"/> interface with features
    ///// that are only avaialble within the PubPropBag assembly.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    //public interface IPropPrivate<T> : IProp<T>
    //{

    //}

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

        Attribute[] Attributes { get; }
    }

    /// <summary>
    /// These are the non-type specific features that every instance of IProp<typeparamref name="T"/> implement.
    /// </summary>
    public interface IProp : ICloneable
    {
        PropKindEnum PropKind { get; }
        Type Type { get; }
        bool TypeIsSolid { get; }
        bool HasStore { get; }

        //IListSource ListSource { get; }
        //ICollectionView CollectionView { get; }

        object TypedValueAsObject { get; }
        ValPlusType GetValuePlusType();

        bool ValueIsDefined { get; }

        /// <summary>
        /// Marks the property as having an undefined value.
        /// </summary>
        /// <returns>True, if the value was defined at the time this call was made.</returns>
        bool SetValueToUndefined();

        void CleanUpTyped();

        bool RegisterBinding(IPropBagInternal propBag, PropIdType propId, LocalBindingInfo bindingInfo);
        bool UnregisterBinding(IPropBagInternal propBag, PropIdType propId, LocalBindingInfo bindingInfo);
    }

    /// <summary>
    /// Allows access from code in the TypeSafePropertyBag assembly, but not from the PropBag assembly.
    /// </summary>
    internal interface IPropDataInternal : IPropData
    {
        ExKeyT CKey { get; }
        bool IsPropBag { get; }

        // On those occasions when the IProp starts off with Type = object, and then later, the type is firmed up,
        // The IPropBag needs to be able to have a new IProp created with the correct type
        // and that new IProp needs to replace the original IProp.
        void SetTypedProp(PropNameType propertyName, IProp value);
    }

    /// <summary>
    /// Classes that implement the IPropBag interface, keep a list of properties, each of which implements this interface.
    /// These features are managed by the PropBag, and not by classes that inherit from AbstractProp.
    /// </summary>
    public interface IPropData
    {
        PropIdType PropId { get; }

        bool IsEmpty { get; }

        /// <summary>
        /// Provides access to the non-type specific features of this property.
        /// This allows access to these values without having to cast to the instance to its type (unknown at compile time.)
        /// </summary>
        IProp TypedProp { get; }

        ValPlusType GetValuePlusType();

        void CleanUp(bool doTypedCleanup);
    }
}
