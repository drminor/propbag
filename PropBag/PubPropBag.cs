using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace DRM.PropBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    using SubCacheType = ICacheSubscriptions<UInt32>;

    #region Summary and Remarks

    /// <remarks>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC.
    /// I have chosen to provide others free access to this intellectual property using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points made here, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used.
    /// </remarks>

    #endregion

    ///<summary>
    /// This class implements IPubPropBag, which allows customers of classes that dervive from this class
    /// the ability to add and remove properties. 
    ///</summary>
    public class PubPropBag : PropBag, IPubPropBag
    {
        #region Constructor

        public PubPropBag(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory = null, string fullClassName = null)
            : base(pm, storeAccessCreator, propFactory, fullClassName)
        {
        }

        protected PubPropBag(IPropBagInternal copySource)
            : base(copySource)
        {
        }

        public PubPropBag(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName = null)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        {
        }

        #endregion

        #region Property Management

        /// <summary>
        /// Use when you want to specify an Action<typeparamref name="T"/> to be performed
        /// either before or after the PropertyChanged event has been raised.
        /// </summary>
        /// <typeparam name="T">The type of this property's value.</typeparam>
        /// <param name="propertyName"></param>
        /// <param name="doIfChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <param name="comparer">A instance of a class that implements IEqualityComparer and thus an Equals method.</param>
        /// <param name="initalValue"></param>
        new public IProp<T> AddProp<T>(string propertyName, Func<T,T,bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            return base.AddProp<T>(propertyName, comparer, extraInfo, initialValue);
        }

        // TODO: Consider removing this method and adding a parameter to AddProp named "UseRefEquality."
        new public IProp<T> AddPropObjComp<T>(string propertyName, object extraInfo = null, T initialValue = default(T))
        {
            return base.AddPropObjComp(propertyName, extraInfo, initialValue);
        }

        new public IProp<T> AddPropNoValue<T>(string propertyName, Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            return base.AddPropNoValue(propertyName, comparer, extraInfo);
        }

        new public IProp<T> AddPropObjCompNoValue<T>(string propertyName, object extraInfo = null)
        {
            return base.AddPropObjCompNoValue<T>(propertyName, extraInfo);
        }

        new public IProp<T> AddPropNoStore<T>(string propertyName, Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            return base.AddPropNoStore(propertyName, comparer, extraInfo);
        }

        new public IProp<T> AddPropObjCompNoStore<T>(string propertyName, object extraInfo = null)
        {
            return base.AddPropObjCompNoStore<T>(propertyName, extraInfo);
        }

        new public ICProp<CT, T> AddCollectionProp<CT, T>(string propertyName, Func<CT, CT, bool> comparer = null,
            object extraInfo = null, CT initialValue = default(CT)) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            return base.AddCollectionProp<CT, T>(propertyName, comparer, extraInfo, initialValue);
        }

        //new public ICPropFB<CT, T> AddCollectionPropFB<CT, T>(string propertyName, Func<CT, CT, bool> comparer = null,
        //    object extraInfo = null, CT initialValue = default(CT)) where CT : ObservableCollection<T>
        //{
        //    return base.AddCollectionPropFB<CT, T>(propertyName, comparer, extraInfo, initialValue);
        //}

        new public void RemoveProp(string propertyName, Type propertyType)
        {
            base.RemoveProp(propertyName, propertyType);
        }

        new public void RemoveProp<T>(string propertyName)
        {
            base.RemoveProp<T>(propertyName);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="propertyName"></param>
        ///// <param name="doWhenChanged"></param>
        ///// <param name="doAfterNotify"></param>
        ///// <returns>True, if there was an existing Action in place for this property.</returns>
        //new public bool RegisterDoWhenChanged<T>(string propertyName, Action<T, T> doWhenChanged, bool doAfterNotify = false)
        //{
        //    return base.RegisterDoWhenChanged(doWhenChanged, doAfterNotify, propertyName);
        //}

        new public void ClearAllProps()
        {
            base.ClearAllProps();
        }

        #endregion
    }
}

