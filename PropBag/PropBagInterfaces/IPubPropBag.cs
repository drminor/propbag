using System;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.ComponentModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag
{
    /// <summary>
    /// Adds AddProp, RemoveProp and ClearAllProps to IPropBag.
    /// </summary>
    public interface IPubPropBag : IPropBag
    {
        //object this[string typeName, string propertyName] { get; set; }

        IProp<T> AddProp<T>(string propertyName, EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropObjComp<T>(string propertyName, EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropNoValue<T>(string propertyName, EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoValue<T>(string propertyName, EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null);

        IProp<T> AddPropNoStore<T>(string propertyName, EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoStore<T>(string propertyName, EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null);

        void RemoveProp(string propertyName, Type propertyType);
        void RemoveProp<T>(string propertyName);

        bool RegisterDoWhenChanged<T>(string propertyName, Action<T, T> doWhenChanged,
            bool doAfterNotify = false);

        void ClearAllProps();

        void ClearEventSubscribers();
    }

}
