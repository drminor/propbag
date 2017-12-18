using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Adds AddProp, RemoveProp and ClearAllProps to IPropBag.
    /// </summary>
    public interface IPubPropBag
    {
        //object this[string typeName, string propertyName] { get; set; }

        IProp<T> AddProp<T>(string propertyName, Func<T,T,bool> comparer = null, object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropObjComp<T>(string propertyName, object extraInfo = null, T initalValue = default(T));

        IProp<T> AddPropNoValue<T>(string propertyName, Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoValue<T>(string propertyName, object extraInfo = null);

        IProp<T> AddPropNoStore<T>(string propertyName, Func<T,T,bool> comparer = null, object extraInfo = null);

        IProp<T> AddPropObjCompNoStore<T>(string propertyName, object extraInfo = null);

        ICPropPrivate<CT, T> AddCollectionProp<CT, T>(string propertyName, Func<CT, CT, bool> comparer = null,
            object extraInfo = null, CT initialValue = default(CT)) where CT : class, IEnumerable<T>;

        void RemoveProp(string propertyName, Type propertyType);
        void RemoveProp<T>(string propertyName);

        // TODO: Create a replacement for this method.
        //bool RegisterDoWhenChanged<T>(string propertyName, Action<T, T> doWhenChanged,
        //    bool doAfterNotify = false);

        void ClearAllProps();
   }

}
