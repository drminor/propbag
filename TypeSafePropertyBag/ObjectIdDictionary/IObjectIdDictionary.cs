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
    public interface IObjectIdDictionary<ExKeyT, CompT, L1T, L2T, L2TRaw, PropDataT>
        : IDictionary<ExKeyT, PropDataT>, ICollection<KeyValuePair<ExKeyT, PropDataT>>, 
        IEnumerable<KeyValuePair<ExKeyT, PropDataT>>, IDictionary, ICollection, IEnumerable where ExKeyT : IExplodedKey<CompT, L1T, L2T> //where PropDataT : IPropGen
    {
        #region ConcurentDictionary Methods

        bool TryAdd(ExKeyT exKey, PropDataT value);

        PropDataT GetOrAdd(ExKeyT cKey, PropDataT value);
        PropDataT GetOrAdd(ExKeyT cKey, Func<ExKeyT, PropDataT> valueFactory);

        // These are provided by IDictionary
        //bool TryGetValue(CompT key, out TValue value);
        //bool ContainsKey(CompT key);
        //TValue this[CompT key] { get; set; }


        bool TryRemove(ExKeyT cKey, out PropDataT value);

        bool TryUpdate(ExKeyT key, PropDataT newValue, PropDataT comparisonValue);
        PropDataT AddOrUpdate(ExKeyT cKey, PropDataT addValue, Func<ExKeyT, PropDataT, PropDataT> updateValueFactory);
        PropDataT AddOrUpdate(ExKeyT cKey, Func<ExKeyT, PropDataT> addValueFactory, Func<ExKeyT, PropDataT, PropDataT> updateValueFactory);

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

        //bool TryUpdate(L1T top, L2T bot, PropDataT newValue, PropDataT comparisonValue);
        //PropDataT AddOrUpdate(L1T top, L2T bot, PropDataT addValue, Func<L1T, L2T, PropDataT, PropDataT> updateValueFactory);
        //PropDataT AddOrUpdate(L1T top, L2T bot, Func<L1T, L2T, PropDataT> addValueFactory, Func<L1T, L2T, PropDataT, PropDataT> updateValueFactory);

        #endregion

        #region Level 2 Raw

        bool TryAdd(L1T top, L2TRaw rawBot, PropDataT value);
        PropDataT GetOrAdd(L1T top, L2TRaw rawBot, PropDataT value);

        //TValue GetOrAdd(L1T top, L2TRaw rawBot, Func<L2TRaw rawBot, TValue> valueFactory);

        bool TryGetValue(L1T top, L2TRaw rawBot, out PropDataT value);
        bool ContainsKey(L1T top, L2TRaw rawBot);
        //TValue thisL1T top, L2TRaw rawBot] { get; set; }


        bool TryRemove(L1T top, L2TRaw rawBot, out PropDataT value);

        //bool TryUpdate(L1T top, L2TRaw rawBot, PropDataT newValue, PropDataT comparisonValue);
        //PropDataT AddOrUpdate(L1T top, L2TRaw rawBot, PropDataT addValue, Func<L1T, L2TRaw, PropDataT, PropDataT> updateValueFactory);
        //PropDataT AddOrUpdate(L1T top, L2TRaw rawBot, Func<L1T, L2TRaw, PropDataT> addValueFactory, Func<L1T, L2TRaw, PropDataT, PropDataT> updateValueFactory);

        #endregion
    }

    public interface ICKeyMan<ExKeyT, CompT, L1T, L2T, L2TRaw> where ExKeyT : IExplodedKey<CompT, L1T, L2T>
    {
        // Join and split exploded key from L1 and L2
        ExKeyT Join(L1T top, L2T bot);
        L1T Split(ExKeyT exKey, out L2T bot);

        // Join and split exploded key from L1 and L2Raw.
        ExKeyT Join(L1T top, L2TRaw bot);
        L1T Split(ExKeyT exKey, out L2TRaw bot);

        // Try version of Join
        bool TryJoin(L1T top, L2TRaw rawBot, out ExKeyT exKey);

        // Try version of Join Comp
        bool TryJoinComp(L1T top, L2TRaw rawBot, out CompT cKey, out L2T bot);

        // Create exploded key from composite key.
        ExKeyT Split(CompT cKey);

        // Join and split composite key from L1 and L2.
        CompT JoinComp(L1T top, L2T bot);
        L1T SplitComp(CompT cKey, out L2T bot);

        // Join and split composite key from L1 and L2Raw.
        CompT JoinComp(L1T top, L2TRaw rawBot);
        L1T SplitComp(CompT cKey, out L2TRaw rawBot);
    }

    public interface IL2KeyMan<L2T, L2TRaw>
    {
        L2T FromRaw(L2TRaw rawBot);
        bool TryGetFromRaw(L2TRaw rawBot, out L2T bot);

        L2TRaw FromCooked(L2T bot);
        bool TryGetFromCooked(L2T raw, out L2TRaw rawBot);

        L2T Add(L2TRaw rawBot);
        L2T GetOrAdd(L2TRaw rawBot);
    }

    //public interface IProvideObjectIds<ExKeyT, CompT, L1T, L2T, L2TRaw> : IIssueL1Keys<L1T> where ExKeyT : IExplodedKey<CompT, L1T, L2T>
    //{
    //    ICKeyMan<ExKeyT, CompT, L1T, L2T, L2TRaw> CompKeyManager { get; }
    //    IL2KeyMan<L2T, L2TRaw> Leve2KeyManager { get; }
    //}

    //public interface IIssueL1Keys<L1T>
    //{
    //    L1T NextL1Key { get; }
    //}

    //public interface IHaveAnL1Key<L1T>
    //{
    //    L1T ObjectId { get; }
    //}
}
