using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using DRM.Inpcwv;

namespace DRM.PropBag
{
    #region Summary and Remarks

    /// <summary>
    /// The contents of this code file were designed and created by David R. Minor, Pittsboro, NC.
    /// I have chosen to provide others free access to this intellectual property using the terms set forth
    /// by the well known Code Project Open License.
    /// Please refer to the file in this same folder named CPOP.htm for the exact set of terms that govern this release.
    /// Although not included as a condition of use, I would prefer that this text, 
    /// or a similar text which covers all of the points, be included along with a copy of cpol.htm
    /// in the set of artifacts deployed with any product
    /// wherein this source code, or a derivative thereof, is used to build the product.
    /// </summary>

    #endregion

    public class PropBag : PropBagBase, IPropBag
    {
        #region Constructor

        public PropBag() { }

        public PropBag(byte dummy) : base(dummy) {}

        public PropBag(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode) {}

        public PropBag(PropBagTypeSafetyMode typeSafetyMode, AbstractPropFactory thePropFactory) : base(typeSafetyMode, thePropFactory) {}

        public PropBag(DRM.PropBag.ControlModel.PropModel pm) : base(pm) { }

        #endregion

        #region Propety Access Methods 

        public new object this[string typeName, string propertyName]
        {
            get { return base[typeName, propertyName]; }
            set { base[typeName, propertyName] = value; }
        }

        public object this[string propertyName]
        {
            get
            {
                return PGetIt(propertyName);
            }
            set
            {
                PSetIt(value, propertyName, null);
            }
        }

        public object GetIt([CallerMemberName] string propertyName = null)
        {
            return base.PGetIt(propertyName);
        }

        public T GetIt<T>([CallerMemberName] string propertyName = null)
        {
            return base.PGetIt<T>(propertyName);
        }

        public void SetIt(object value, [CallerMemberName] string propertyName = null)
        {
            base.PSetIt(value, propertyName, null);
        }

        public void SetIt<T>(T value, [CallerMemberName] string propertyName = null)
        {
            base.PSetIt<T>(value, propertyName);
        }

        /// <summary>
        /// For use when the Property Bag's internal storage is not appropriate. This allows
        /// the property implementor to use a backing store of their choice.
        /// The property must be registered with a call to AddPropNoStore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newValue">The new value to use to update the property. No operation will be performed if this value is the same as the current value.</param>
        /// <param name="curValue">The current value of the property, must be specified using the ref keyword.</param>
        /// <param name="propertyName"></param>
        /// <returns>True if the value was updated, otherwise false.</returns>
        public bool SetIt<T>(T newValue, ref T curValue, [CallerMemberName]string propertyName = null)
        {
            return base.PSetIt<T>(newValue, ref curValue, propertyName);
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
        protected IProp<T> AddProp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null, T initialValue = default(T))
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            IProp<T> pg = ThePropFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            base.AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjComp<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null, T initialValue = default(T))
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            Func<T,T,bool> comparer = ThePropFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = ThePropFactory.Create<T>(initialValue, propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoValue<T>(string propertyName, Action<T, T> doIfChanged = null, bool doAfterNotify = false,
            object extraInfo = null)
        {
            bool hasStorage = true;
            bool typeIsSolid = true;
            Func<T,T,bool> comparer = ThePropFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            Func<T,T,bool> comparer = null, object extraInfo = null)
        {
            bool hasStorage = false;
            bool typeIsSolid = true;
            IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected IProp<T> AddPropObjCompNoStore<T>(string propertyName, Action<T, T> doIfChanged, bool doAfterNotify = false,
            object extraInfo = null)
        {
            bool hasStorage = false;
            bool typeIsSolid = true;
            Func<T,T,bool> comparer = ThePropFactory.GetRefEqualityComparer<T>();
            IProp<T> pg = ThePropFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, typeIsSolid, doIfChanged, doAfterNotify, comparer);
            AddProp<T>(propertyName, pg);
            return pg;
        }

        protected void RemoveProp(string propertyName)
        {
            base.PRemoveProp(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doWhenChanged"></param>
        /// <param name="doAfterNotify"></param>
        /// <param name="propertyName"></param>
        /// <returns>True, if there was an existing Action in place for this property.</returns>
        protected bool RegisterDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify = false, [CallerMemberName] string propertyName = null)
        {
            return base.PRegisterDoWhenChanged(doWhenChanged, doAfterNotify, propertyName);    
        }

        /// <summary>
        /// Returns all of the values in dictionary of objects, keyed by PropertyName.
        /// </summary>
        public IDictionary<string, object> GetAllPropertyValues()
        {
            return base.PGetAllPropertyValues();
        }

        public IList<string> GetAllPropertyNames()
        {
            return base.PGetAllPropertyNames();
        }

        protected void ClearAll()
        {
            base.PClearAll();
        }

        protected void ClearEventSubscribers()
        {
            base.PClearEventSubscribers();
        }

        #endregion

        #region Methods to Raise Events

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName">Use "Item[]" if you want to notify the WPF binding system that one of the PropBag properties has changed.</param>
        new protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName">Use "Item[]" if you want to notify the WPF binding system that one of the PropBag properties is changing.</param>
        protected void OnPropertyChanging([CallerMemberName]string propertyName = null)
        {
            base.POnPropertyChanging(propertyName);
        }

        protected void OnPropertyChangedWithVals(object oldVal, object newVal, [CallerMemberName]string propertyName = null)
        {
            base.POnPropertyChangedWithVals(propertyName, oldVal, newVal);
        }

        #endregion
    }
}
