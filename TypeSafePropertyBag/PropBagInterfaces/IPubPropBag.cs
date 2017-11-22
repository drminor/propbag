﻿using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Adds AddProp, RemoveProp and ClearAllProps to IPropBag.
    /// </summary>
    public interface IPubPropBag
    {
        //object this[string typeName, string propertyName] { get; set; }

        IProp<T> AddProp<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropObjComp<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropNoValue<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoValue<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null);

        IProp<T> AddPropNoStore<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoStore<T>(string propertyName, EventHandler<PCTypedEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false,
            object extraInfo = null);

        void RemoveProp(string propertyName, Type propertyType);
        void RemoveProp<T>(string propertyName);

        bool RegisterDoWhenChanged<T>(string propertyName, Action<T, T> doWhenChanged,
            bool doAfterNotify = false);

        void ClearAllProps();

        void ClearEventSubscribers();
    }

}