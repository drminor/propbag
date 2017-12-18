using System;
using System.Collections;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="CompT">The type used for global property identifiers, where properties are instances of IProp. Global typically means an Application Domain</typeparam>
    /// <typeparam name="L1T">The type used for object identifiers. Where objects are instances of IPropBag.</typeparam>
    /// <typeparam name="L2T">The type used to  identify a property for a given object using the raw form, usually a numeric type.</typeparam>
    /// <typeparam name="L2TRaw">The type used to identify a property for a given object using the "cooked" form, usually System.String.</typeparam>
    /// <typeparam name="PropDataT">The type used to implement the IPropGen interface.</typeparam>
    public interface IObjectIdDictionary<CompT, L1T, L2T, L2TRaw, PropDataT>
        : IDictionary<CompT, PropDataT>,
        ICollection<KeyValuePair<CompT, PropDataT>>, 
        IEnumerable<KeyValuePair<CompT, PropDataT>>,
        IDictionary, ICollection, IEnumerable 
    {

        // Readonly access to the Composite Key Manager.
        //ICKeyMan<CompT, L1T, L2T, L2TRaw> CompKeyManager { get; }

        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        #region ConcurentDictionary Methods

        bool TryAdd(CompT cKey, PropDataT value);

        PropDataT GetOrAdd(CompT cKey, PropDataT value);
        PropDataT GetOrAdd(CompT cKey, Func<CompT, PropDataT> valueFactory);

        // These are provided by IDictionary
        //bool TryGetValue(CompT key, out TValue value);
        //bool ContainsKey(CompT key);
        //TValue this[CompT key] { get; set; }


        bool TryRemove(CompT cKey, out PropDataT value);

        bool TryUpdate(CompT key, PropDataT newValue, PropDataT comparisonValue);
        PropDataT AddOrUpdate(CompT cKey, PropDataT addValue, Func<CompT, PropDataT, PropDataT> updateValueFactory);
        PropDataT AddOrUpdate(CompT cKey, Func<CompT, PropDataT> addValueFactory, Func<CompT, PropDataT, PropDataT> updateValueFactory);

        #endregion

        #region Level 1

        int RemoveAll(L1T top);

        #endregion

        #region Level 2 Cooked

        //bool TryAdd(L1T top, L2T bot, PropDataT value);
        //PropDataT GetOrAdd(L1T top, L2T bot, PropDataT value);
        ////TValue GetOrAdd(L1T top, L2T bot, Func<L1T top, L2T bot, TValue> valueFactory);

        //bool TryGetValue(L1T top, L2T bot, out PropDataT value);
        //bool ContainsKey(L1T top, L2T bot);
        ////TValue thisL1T top, L2T bot] { get; set; }

        //bool TryRemove(L1T top, L2T bot, out PropDataT value);

        ////bool TryUpdate(L1T top, L2T bot, PropDataT newValue, PropDataT comparisonValue);
        ////PropDataT AddOrUpdate(L1T top, L2T bot, PropDataT addValue, Func<L1T, L2T, PropDataT, PropDataT> updateValueFactory);
        ////PropDataT AddOrUpdate(L1T top, L2T bot, Func<L1T, L2T, PropDataT> addValueFactory, Func<L1T, L2T, PropDataT, PropDataT> updateValueFactory);

        #endregion

        //#region Level 2 Raw

        //bool TryAdd(L1T top, L2TRaw rawBot, PropDataT value);
        //PropDataT GetOrAdd(L1T top, L2TRaw rawBot, PropDataT value);

        ////TValue GetOrAdd(L1T top, L2TRaw rawBot, Func<L2TRaw rawBot, TValue> valueFactory);

        //bool TryGetValue(L1T top, L2TRaw rawBot, out PropDataT value);
        //bool ContainsKey(L1T top, L2TRaw rawBot);
        ////TValue thisL1T top, L2TRaw rawBot] { get; set; }


        //bool TryRemove(L1T top, L2TRaw rawBot, out PropDataT value);

        ////bool TryUpdate(L1T top, L2TRaw rawBot, PropDataT newValue, PropDataT comparisonValue);
        ////PropDataT AddOrUpdate(L1T top, L2TRaw rawBot, PropDataT addValue, Func<L1T, L2TRaw, PropDataT, PropDataT> updateValueFactory);
        ////PropDataT AddOrUpdate(L1T top, L2TRaw rawBot, Func<L1T, L2TRaw, PropDataT> addValueFactory, Func<L1T, L2TRaw, PropDataT, PropDataT> updateValueFactory);

        //#endregion
    }
}
