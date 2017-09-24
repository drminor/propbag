using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class PropFactory : AbstractPropFactory
    {

        public PropFactory(bool returnDefaultForUndefined) : base(returnDefaultForUndefined) { }

        public override bool ProvidesStorage
        {
            get { return true; }
        }

        public override IProp<T> Create<T>(
            T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            IProp<T> prop = new Prop<T>(initialValue, hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            IProp<T> prop = new Prop<T>(hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IPropGen CreateGenFromObject(Type typeOfThisProperty,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            CreatePropFromObjectDelegate propCreator = GetPropCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid, 
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
            return prop;
        }

        public override IPropGen CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
            return prop;
        }

        public override IPropGen CreateGenWithNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
            return prop;
        }

    }

}
