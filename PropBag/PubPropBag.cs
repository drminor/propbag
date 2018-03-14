using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.PropBag
{
    using PropNameType = String;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

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
    /// This class implements IPubPropBag, which allows customers of this class (and classes that dervive from this class)
    /// to add and remove properties. 
    ///</summary>
    public class PubPropBag : PropBag, IPubPropBag
    {
        #region Constructor

        // Starting with an empty PropItemSet
        public PubPropBag(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactory propFactory)
            : this(typeSafetyMode, storeAccessCreator, propFactory, fullClassName: null)
        {
        }

        public PubPropBag(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAccessCreator,
            IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, storeAccessCreator, propFactory, fullClassName)
        {
        }

        protected PubPropBag(PropBagTypeSafetyMode typeSafetyMode, PSAccessServiceCreatorInterface storeAcessorCreator,
            IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
            : base(typeSafetyMode, storeAcessorCreator, autoMapperService, propFactory, fullClassName)
        {
        }

        // Using a PropModel
        public PubPropBag(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator)
            : this(pm, storeAccessCreator, propFactory: null, fullClassName: null)
        {
        }

        public PubPropBag(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory)
            : this(pm, storeAccessCreator, propFactory, fullClassName: null)
        {
        }

        public PubPropBag(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, string fullClassName)
            : this(pm, storeAccessCreator, propFactory: null, fullClassName: fullClassName)
        {
        }

        public PubPropBag(PropModel pm, PSAccessServiceCreatorInterface storeAccessCreator, IPropFactory propFactory, string fullClassName)
            : base(pm, storeAccessCreator, propFactory, fullClassName)
        {
        }

        #endregion

        new public PropBagTypeSafetyMode TypeSafetyMode => base.TypeSafetyMode;
        new public bool IsPropSetFixed => base.IsPropSetFixed;
        new public bool TryFixPropSet() => base.TryFixPropSet();
        new public bool TryOpenPropSet() => base.TryOpenPropSet();

        #region Property Management

        new public IProp<T> AddProp<T>(string propertyName)
        {
            IProp<T> result = AddProp<T>(propertyName, comparer: null, extraInfo: null, initialValue: default(T));
            return result;
        }

        new public IProp<T> AddProp<T>(string propertyName, T initialValue)
        {
            IProp<T> result = AddProp<T>(propertyName, comparer: null, extraInfo: null, initialValue: initialValue);
            return result;
        }

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
        new public IProp<T> AddProp<T>(PropNameType propertyName, Func<T,T,bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            return base.AddProp<T>(propertyName, comparer, extraInfo, initialValue);
        }

        // TODO: Consider removing this method and adding a parameter to AddProp named "UseRefEquality."
        new public IProp<T> AddPropObjComp<T>(PropNameType propertyName, object extraInfo = null, T initialValue = default(T))
        {
            return base.AddPropObjComp(propertyName, extraInfo, initialValue);
        }

        new public IProp<T> AddPropNoValue<T>(PropNameType propertyName, Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            return base.AddPropNoValue(propertyName, comparer, extraInfo);
        }

        new public IProp<T> AddPropObjCompNoValue<T>(PropNameType propertyName, object extraInfo = null)
        {
            return base.AddPropObjCompNoValue<T>(propertyName, extraInfo);
        }

        new public IProp<T> AddPropNoStore<T>(PropNameType propertyName, Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            return base.AddPropNoStore(propertyName, comparer, extraInfo);
        }

        new public IProp<T> AddPropObjCompNoStore<T>(PropNameType propertyName, object extraInfo = null)
        {
            return base.AddPropObjCompNoStore<T>(propertyName, extraInfo);
        }

        new public ICProp<CT, T> AddCollectionProp<CT, T>(PropNameType propertyName, Func<CT, CT, bool> comparer = null,
            object extraInfo = null, CT initialValue = default(CT)) where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            return base.AddCollectionProp<CT, T>(propertyName, comparer, extraInfo, initialValue);
        }

        new public bool TryRemoveProp(PropNameType propertyName, Type propertyType)
        {
            return base.TryRemoveProp(propertyName, propertyType);
        }

        new public bool TryRemoveProp<T>(PropNameType propertyName)
        {
            return base.TryRemoveProp<T>(propertyName);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="propertyName"></param>
        ///// <param name="doWhenChanged"></param>
        ///// <param name="doAfterNotify"></param>
        ///// <returns>True, if there was an existing Action in place for this property.</returns>
        //new public bool RegisterDoWhenChanged<T>(PropNameType propertyName, Action<T, T> doWhenChanged, bool doAfterNotify = false)
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

