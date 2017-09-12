using System;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using System.ComponentModel;

using DRM.Inpcwv;

namespace DRM.PropBag
{
    /// <summary>
    /// Adds GetIt, SetIt, and AddProp methods to IPropBag.
    /// </summary>
    public interface IPubPropBag : IPropBag
    {

        //void AddToPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, [CallerMemberName] string eventPropertyName = null);
        //void RemoveFromPropChanged<T>(PropertyChangedWithTValsHandler<T> eventHandler, [CallerMemberName] string eventPropertyName = null);

        IProp<T> AddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null);

        IProp<T> AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            object extraInfo = null);

        void RemoveProp(string propertyName);

        bool RegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify = false,
            [CallerMemberName] string propertyName = null);

        void ClearAll();

        void ClearEventSubscribers();

    }

}
