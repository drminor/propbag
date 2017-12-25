using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public interface ICreateProps
    {
        ICProp<CT, T> CreateCPropFromObject<CT, T>(IPropFactory propFactory, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        ICProp<CT, T> CreateCPropFromString<CT, T>(IPropFactory propFactory, string value, bool useDefault, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = true) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        ICProp<CT, T> CreateCPropWithNoValue<CT, T>(IPropFactory propFactory, bool useDefault, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = true) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        T CreateNewUsingDefaultConstructor<T>();

        IProp<T> CreatePropFromObject<T>(IPropFactory propFactory, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false);
        IProp<T> CreatePropFromString<T>(IPropFactory propFactory, string value, bool useDefault, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false);
        IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false);

        Func<CT, CT, bool> GetComparerForCollections<CT>(Delegate comparer, IPropFactory propFactory, bool useRefEquality);
        Func<T, T, bool> GetComparerForProps<T>(Delegate comparer, IPropFactory propFactory, bool useRefEquality);
    }
}