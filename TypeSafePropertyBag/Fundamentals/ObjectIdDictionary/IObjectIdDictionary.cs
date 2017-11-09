using System;
using System.Collections;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
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
        : IDictionary<CompT, PropDataT>, ICollection<KeyValuePair<CompT, PropDataT>>, 
        IEnumerable<KeyValuePair<CompT, PropDataT>>, IDictionary, ICollection, IEnumerable where PropDataT : IPropGen
    {
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

        bool TryAdd(L1T top, L2T bot, PropDataT value);
        PropDataT GetOrAdd(L1T top, L2T bot, PropDataT value);
        //TValue GetOrAdd(L1T top, L2T bot, Func<L1T top, L2T bot, TValue> valueFactory);

        bool TryGetValue(L1T top, L2T bot, out PropDataT value);
        bool ContainsKey(L1T top, L2T bot);
        //TValue thisL1T top, L2T bot] { get; set; }

        bool TryRemove(L1T top, L2T bot, out PropDataT value);

        //bool TryUpdate(L1T top, L2T bot, TValue newValue, TValue comparisonValue);
        //TValue AddOrUpdate(L1T top, L2T bot, TValue addValue, Func<L1T top, L2T bot, TValue, TValue> updateValueFactory);
        //TValue AddOrUpdate(L1T top, L2T bot, Func<L1T top, L2T bot, TValue> addValueFactory, Func<L1T top, L2T bot, TValue, TValue> updateValueFactory);

        #endregion

        #region Level 2 Raw

        bool TryAdd(L1T top, L2TRaw rawBot, PropDataT value);
        PropDataT GetOrAdd(L1T top, L2TRaw rawBot, PropDataT value);

        //TValue GetOrAdd(L1T top, L2TRaw rawBot, Func<L2TRaw rawBot, TValue> valueFactory);

        bool TryGetValue(L1T top, L2TRaw rawBot, out PropDataT value);
        bool ContainsKey(L1T top, L2TRaw rawBot);
        //TValue thisL1T top, L2TRaw rawBot] { get; set; }


        bool TryRemove(L1T top, L2TRaw rawBot, out PropDataT value);

        //bool TryUpdate(L1T top, L2TRaw rawBot, TValue newValue, TValue comparisonValue);
        //TValue AddOrUpdate(L1T top, L2TRaw rawBot, TValue addValue, Func<L1T top, L2TRaw rawBot, TValue, TValue> updateValueFactory);
        //TValue AddOrUpdate(L1T top, L2TRaw rawBot, Func<L1T top, L2TRaw rawBot, TValue> addValueFactory, Func<L1T top, L2TRaw rawBot, TValue, TValue> updateValueFactory);

        #endregion
    }

    public interface IProvideObjectIds<CompT, L1T, L2T, L2TRaw> : IIssueL1Keys<L1T>
    {
        ICKeyMan<CompT, L1T, L2T, L2TRaw> CompKeyManager { get; }
        IL2KeyMan<L2T, L2TRaw> Leve2KeyManager { get; }
    }

    public interface IIssueL1Keys<L1T>
    {
        L1T NextL1Key { get; }
    }

    public interface ICKeyMan<CompT, L1T, L2T, L2TRaw>
    {
        CompT Join(L1T top, L2T bot);
        L1T Split(CompT cKey, out L2T bot);

        IExplodedKey<CompT, L1T, L2T> Join(L1T top, L2TRaw rawBot);
        L1T Split(CompT cKey, out L2TRaw rawBot);

        bool TryJoin(L1T top, L2TRaw rawBot, out CompT cKey);

        IExplodedKey<CompT, L1T, L2T> Split(CompT cKey);
    }

    public interface IL2KeyMan<L2T, L2TRaw>
    {
        L2T FromRaw(L2TRaw rawBot);
        bool TryGetFromRaw(L2TRaw rawBot, out L2T bot);

        L2TRaw FromCooked(L2T bot);
        bool TryGetFromCooked(L2T raw, out L2TRaw rawBot);

        L2T Add(L2TRaw rawBot);
    }

    public interface IExplodedKey<CompT, L1T, L2T>
    {
        CompT CKey { get; }
        L1T Level1Key { get; }
        L2T Level2Key { get; }
    }

    //public interface IHaveAnL1Key<L1T>
    //{
    //    L1T ObjectId { get; }
    //}
}
