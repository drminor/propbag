using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    // TODO: This interface, may, at some point, be implemented by DRM.TypeSafePropertyBag.APFGenericMethodTemplates

    public interface ICreateProps
    {
        ICProp<CT, T> CreateCPropFromObject<CT, T>(IPropFactory propFactory, object value, PropNameType propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        ICProp<CT, T> CreateCPropFromString<CT, T>(IPropFactory propFactory, string value, bool useDefault, PropNameType propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = true) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;
        ICProp<CT, T> CreateCPropWithNoValue<CT, T>(IPropFactory propFactory, bool useDefault, PropNameType propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = true) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged;

        T CreateNewUsingDefaultConstructor<T>();

        IProp<T> CreatePropFromObject<T>(IPropFactory propFactory, object value, PropNameType propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false);
        IProp<T> CreatePropFromString<T>(IPropFactory propFactory, string value, bool useDefault, PropNameType propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false);
        IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory, PropNameType propertyName, object extraInfo, bool hasStorage, bool isTypeSolid, Delegate comparer, bool useRefEquality = false);

        Func<CT, CT, bool> GetComparerForCollections<CT>(Delegate comparer, IPropFactory propFactory, bool useRefEquality);
        Func<T, T, bool> GetComparerForProps<T>(Delegate comparer, IPropFactory propFactory, bool useRefEquality);

        IProp CreateCVSProp(PropNameType propertyName, IProvideAView viewProvider);

        IProp CreateCVProp(PropNameType propertyName, IProvideAView viewProvider);

        IProp CreateCVPropFromString(Type typeofThisProperty, PropNameType propertyName, IProvideAView viewProvider);

    }
}