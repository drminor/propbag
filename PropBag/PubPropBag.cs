using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace DRM.PropBag
{
    #region Summary and Remarks

    /// <summary>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC.
    /// I have chosen to provide others free access to this intellectual property using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points made here, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used.
    /// </summary>

    #endregion

    public class PubPropBag : PropBagBase, IPubPropBag
    {
        #region Constructor

        public PubPropBag() { }

        public PubPropBag(byte dummy) : base(dummy) { }

        public PubPropBag(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode) {} 

        public PubPropBag(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory) : base(typeSafetyMode, thePropFactory) { }

        public PubPropBag(DRM.PropBag.ControlModel.PropModel pm) : base(pm) { }

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
        public new IProp<T> AddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            return base.AddProp<T>(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo, initialValue);
            //bool hasStorage = true;
            //bool typeIsSolid = true;
            //IProp<T> pg = ThePropFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            //AddProp<T>(propertyName, pg);
            //return pg;
        }

        // TODO: Consider removing this method and adding a parameter to AddProp named "UseRefEquality."
        public new IProp<T> AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null, T initialValue = default(T))
        {
            return base.AddPropObjComp(propertyName, doIfChanged, doAfterNotify, extraInfo, initialValue);
            //bool hasStorage = true;
            //bool typeIsSolid = true;
            //Func<T,T,bool> comparer = ThePropFactory.GetRefEqualityComparer<T>();
            //IProp<T> pg = ThePropFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            //AddProp<T>(propertyName, pg);
            //return pg;
        }

        public new IProp<T> AddPropNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            return base.AddPropNoValue(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo);
            //bool hasStorage = true;
            //bool typeIsSolid = true;
            //IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            //AddProp<T>(propertyName, pg);
            //return pg;
        }

        public new IProp<T> AddPropObjCompNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            return base.AddPropObjCompNoValue(propertyName, doIfChanged, doAfterNotify, extraInfo);
            //bool hasStorage = true;
            //bool typeIsSolid = true;
            //Func<T,T,bool> comparer = ThePropFactory.GetRefEqualityComparer<T>();
            //IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            //AddProp<T>(propertyName, pg);
            //return pg;
        }

        public new IProp<T> AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            return base.AddPropNoStore(propertyName, doIfChanged, doAfterNotify, comparer, extraInfo);
            //bool hasStorage = false;
            //bool typeIsSolid = true;
            //IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            //AddProp<T>(propertyName, pg);
            //return pg;
        }

        public new IProp<T> AddPropObjCompNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            object extraInfo = null)
        {
            return base.AddPropObjCompNoStore(propertyName, doIfChanged, doAfterNotify, extraInfo);
            //bool hasStorage = false;
            //bool typeIsSolid = true;
            //Func<T,T,bool> comparer = ThePropFactory.GetRefEqualityComparer<T>();
            //IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            //AddProp<T>(propertyName, pg);
            //return pg;
        }

        new public void RemoveProp(string propertyName)
        {
            base.RemoveProp(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="doWhenChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <returns>True, if there was an existing Action in place for this property.</returns>
        new public bool RegisterDoWhenChanged<T>(string propertyName, Action<T, T> doWhenChanged, bool doAfterNotify = false)
        {
            return base.RegisterDoWhenChanged(doWhenChanged, doAfterNotify, propertyName);
        }

        new public void ClearAll()
        {
            base.ClearAll();
        }

        new public void ClearEventSubscribers()
        {
            base.ClearEventSubscribers();
        }

        #endregion
    }
}

