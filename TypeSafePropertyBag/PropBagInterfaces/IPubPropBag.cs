using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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


        ICProp<CT, T> AddCollectionProp<CT, T>(string propertyName, Func<CT, CT, bool> comparer = null,
            object extraInfo = null, CT initialValue = default(CT)) where CT : class, IObsCollection<T>;

        //ICPropFB<CT, T> AddCollectionPropFB<CT, T>(string propertyName, Func<CT, CT, bool> comparer = null,
        //    object extraInfo = null, CT initialValue = default(CT)) where CT : ObservableCollection<T>;

        void RemoveProp(string propertyName, Type propertyType);
        void RemoveProp<T>(string propertyName);

        void ClearAllProps();
   }

}
