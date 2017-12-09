using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;


    //public class APFGenericMethodTemplates : ICreateProps
    //{
    //    #region Collection-Type Methods

    //    // From Object
    //    public ICPropPrivate<CT, T> CreateCPropFromObject<CT,T>(IPropFactory propFactory,
    //        object value,
    //        string propertyName, object extraInfo,
    //        bool hasStorage, bool isTypeSolid,
    //        Delegate comparer, bool useRefEquality = false) where CT : class, IEnumerable<T>
    //    {
    //        CT initialValue = propFactory.GetValueFromObject<CT>(value);

    //        //EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = GetTypedDoWhenChanged<CT>(doWhenChanged);

    //        return propFactory.Create<CT,T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
    //            GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
    //    }

    //    // From String
    //    public ICPropPrivate<CT, T> CreateCPropFromString<CT, T>(IPropFactory propFactory,
    //        string value, bool useDefault,
    //        string propertyName, object extraInfo,
    //        bool hasStorage, bool isTypeSolid,
    //        Delegate comparer, bool useRefEquality = true) where CT : class,IEnumerable<T>
    //    {
    //        CT initialValue;
    //        if (useDefault)
    //        {
    //            initialValue = propFactory.ValueConverter.GetDefaultValue<CT,T>(propertyName);
    //        }
    //        else
    //        {
    //            initialValue = propFactory.GetValueFromString<CT,T>(value);
    //        }
    //        //EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = GetTypedDoWhenChanged<CT>(doWhenChanged);

    //        return propFactory.Create<CT, T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
    //            GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
    //    }

    //    // With No Value
    //    public ICPropPrivate<CT, T> CreateCPropWithNoValue<CT, T>(IPropFactory propFactory,
    //        bool useDefault,
    //        PropNameType propertyName, object extraInfo,
    //        bool hasStorage, bool isTypeSolid,
    //        Delegate comparer, bool useRefEquality = true) where CT : class, IEnumerable<T>
    //    {
    //        //EventHandler<PCTypedEventArgs<CT>> doWhenChangedX = GetTypedDoWhenChanged<CT>(doWhenChanged);

    //        return propFactory.CreateWithNoValue<CT, T>(propertyName, extraInfo, hasStorage, isTypeSolid,
    //            GetComparerForCollections<CT>(comparer, propFactory, useRefEquality));
    //    }

    //    public Func<CT, CT, bool> GetComparerForCollections<CT>(Delegate comparer, IPropFactory propFactory, bool useRefEquality)
    //    {
    //        Func<CT, CT, bool> compr;
    //        if (useRefEquality || comparer == null)
    //            return propFactory.GetRefEqualityComparer<CT>();
    //        else
    //            return compr = (Func<CT, CT, bool>)comparer;
    //    }

    //    #endregion

    //    #region Property-Type Methods

    //    // From Object
    //    public IProp<T> CreatePropFromObject<T>(IPropFactory propFactory,
    //        object value,
    //        string propertyName, object extraInfo,
    //        bool hasStorage, bool isTypeSolid,
    //        Delegate comparer, bool useRefEquality = false)
    //    {
    //        T initialValue = propFactory.GetValueFromObject<T>(value);

    //        //EventHandler<PCTypedEventArgs<T>> doWhenChangedX = GetTypedDoWhenChanged<T>(doWhenChanged);

    //        return propFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
    //            GetComparerForProps<T>(comparer, propFactory, useRefEquality));
    //    }

    //    // From String
    //    public IProp<T> CreatePropFromString<T>(IPropFactory propFactory,
    //        string value, bool useDefault,
    //        string propertyName, object extraInfo,
    //        bool hasStorage, bool isTypeSolid,
    //        Delegate comparer, bool useRefEquality = false)
    //    {
    //        //// TODO: We need another parameter in the Create method to avoid using this 'magic' string.
    //        //string MAGIC_VAL = "-0.-0.-0";

    //        T initialValue;
    //        if (useDefault)
    //        {
    //            initialValue = propFactory.ValueConverter.GetDefaultValue<T>(propertyName);
    //        }
    //        //else if(value.StartsWith(MAGIC_VAL))
    //        //{
    //        //    string[] mvParts = value.Split(':');
    //        //    if(mvParts.Length == 1)
    //        //    {
    //        //        initialValue = CreateNewUsingDefaultConstructor<T>();
    //        //    }
    //        //    else if(mvParts.Length == 2)
    //        //    {
    //        //        // Create a new PropBag using the resourceKey
    //        //        string resourceKey = mvParts[1];
    //        //        throw new NotSupportedException("The propfactory cannot create new PropBags from ResourceKeys.");
    //        //    }
    //        //    else
    //        //    {
    //        //        throw new InvalidOperationException($"The value begins with {MAGIC_VAL}, but has an invalid format.");
    //        //    }
    //        //}
    //        else
    //        {
    //            initialValue = propFactory.GetValueFromString<T>(value);
    //        }

    //        //EventHandler<PCTypedEventArgs<T>> doWhenChangedX = GetTypedDoWhenChanged<T>(doWhenChanged);

    //        return propFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, isTypeSolid,
    //            GetComparerForProps<T>(comparer, propFactory, useRefEquality));
    //    }

    //    public T CreateNewUsingDefaultConstructor<T>()
    //    {
    //        T result = Activator.CreateInstance<T>();
    //        return result;
    //    }

    //    // With No Value
    //    public IProp<T> CreatePropWithNoValue<T>(IPropFactory propFactory,
    //        string propertyName, object extraInfo,
    //        bool hasStorage, bool isTypeSolid,
    //        Delegate comparer, bool useRefEquality = false)
    //    {
    //        //EventHandler<PCTypedEventArgs<T>> doWhenChangedX = GetTypedDoWhenChanged<T>(doWhenChanged);

    //        return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid,
    //            GetComparerForProps<T>(comparer, propFactory, useRefEquality));
    //    }

    //    public Func<T, T, bool> GetComparerForProps<T>(Delegate comparer, IPropFactory propFactory, bool useRefEquality)
    //    {
    //        if (useRefEquality)
    //        {
    //            return propFactory.GetRefEqualityComparer<T>();
    //        }
    //        else if (comparer == null)
    //        {
    //            return null;
    //        }
    //        else
    //        {
    //            return (Func<T, T, bool>)comparer;
    //        }
    //    }

    //    //// TODO: Consider caching the delegates created here, or having creating a new delegate
    //    //// that calls the first.

    //    //// Also remember that if the source delegate has multiple targets that the Target and Method properties only return the values for the last target in the invocation list. 
    //    //public EventHandler<PCTypedEventArgs<T>> GetTypedDoWhenChanged<T>(EventHandler<PCGenEventArgs> doWhenChanged)
    //    //{
    //    //    EventHandler<PCTypedEventArgs<T>> result;
    //    //    if (doWhenChanged == null)
    //    //    {
    //    //        result = null;
    //    //    }
    //    //    else
    //    //    {
    //    //        result = (EventHandler<PCTypedEventArgs<T>>)Delegate.CreateDelegate(typeof(
    //    //            EventHandler<PCTypedEventArgs<T>>), doWhenChanged.Target, doWhenChanged.Method);

    //    //        // This creates a new delegate which calls the first one.
    //    //        //var x = new EventHandler<PropertyChangedWithTValsEventArgs<T>>(doWhenChanged);

    //    //    }
    //    //    return result;
    //    //}

    //    #endregion
    //}
}

