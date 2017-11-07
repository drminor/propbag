using System;
using System.Collections;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary
{
    public interface IObjectIdDictionary<CompT, L1T, L2T, L2TRaw, TValue>
        : IDictionary<CompT, TValue>, ICollection<KeyValuePair<CompT, TValue>>, 
        IEnumerable<KeyValuePair<CompT, TValue>>, IDictionary, ICollection, IEnumerable
    {
        #region ConcurentDictionary Methods

        bool TryAdd(CompT key, TValue value);

        TValue GetOrAdd(CompT key, TValue value);
        TValue GetOrAdd(CompT key, Func<CompT, TValue> valueFactory);

        // These are provided by IDictionary
        //bool TryGetValue(CompT key, out TValue value);
        //bool ContainsKey(CompT key);
        //TValue this[CompT key] { get; set; }


        bool TryRemove(CompT key, out TValue value);

        bool TryUpdate(CompT key, TValue newValue, TValue comparisonValue);
        TValue AddOrUpdate(CompT key, TValue addValue, Func<CompT, TValue, TValue> updateValueFactory);
        TValue AddOrUpdate(CompT key, Func<CompT, TValue> addValueFactory, Func<CompT, TValue, TValue> updateValueFactory);

        #endregion

        #region Level 1

        int RemoveAll(L1T top);

        #endregion

        #region Level 2 Cooked

        bool TryAdd(L1T top, L2T bot, TValue value);
        TValue GetOrAdd(L1T top, L2T bot, TValue value);
        //TValue GetOrAdd(L1T top, L2T bot, Func<L1T top, L2T bot, TValue> valueFactory);

        bool TryGetValue(L1T top, L2T bot, out TValue value);
        bool ContainsKey(L1T top, L2T bot);
        //TValue thisL1T top, L2T bot] { get; set; }

        bool TryRemove(L1T top, L2T bot, out TValue value);

        //bool TryUpdate(L1T top, L2T bot, TValue newValue, TValue comparisonValue);
        //TValue AddOrUpdate(L1T top, L2T bot, TValue addValue, Func<L1T top, L2T bot, TValue, TValue> updateValueFactory);
        //TValue AddOrUpdate(L1T top, L2T bot, Func<L1T top, L2T bot, TValue> addValueFactory, Func<L1T top, L2T bot, TValue, TValue> updateValueFactory);

        #endregion

        #region Level 2 Raw

        bool TryAdd(L1T top, L2TRaw rawBot, TValue value);
        TValue GetOrAdd(L1T top, L2TRaw rawBot, TValue value);

        //TValue GetOrAdd(L1T top, L2TRaw rawBot, Func<L2TRaw rawBot, TValue> valueFactory);

        bool TryGetValue(L1T top, L2TRaw rawBot, out TValue value);
        bool ContainsKey(L1T top, L2TRaw rawBot);
        //TValue thisL1T top, L2TRaw rawBot] { get; set; }


        bool TryRemove(L1T top, L2TRaw rawBot, out TValue value);

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
        L1T Split(CompT comp, out L2T bot);

        CompT Join(L1T top, L2TRaw rawBot);
        L1T Split(CompT comp, out L2TRaw rawBot);

        bool TryJoin(L1T top, L2TRaw rawBot, out CompT comp);
    }

    public interface IL2KeyMan<L2T, L2TRaw>
    {
        L2T FromRaw(L2TRaw rawBot);
        bool TryGetFromRaw(L2TRaw rawBot, out L2T bot);

        L2TRaw FromCooked(L2T bot);
        bool TryGetFromCooked(L2T raw, out L2TRaw rawBot);

        L2T Add(L2TRaw rawBot);
    }

    //public interface IHaveAnL1Key<L1T>
    //{
    //    L1T ObjectId { get; }
    //}
}
