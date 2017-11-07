using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class PropFactory : AbstractPropFactory
    {
        public override bool ProvidesStorage
        {
            get { return true; }
        }

        public PropFactory(/*bool returnDefaultForUndefined,*/
            ResolveTypeDelegate typeResolver = null,
            IConvertValues valueConverter = null)
        : base(/*returnDefaultForUndefined, */typeResolver, valueConverter) { }


        // TODO: This is temporary just for testing.
        //public override Type GetTypeFromName(string typeName)
        //{
        //    throw new ApplicationException("All instances of PropFactory need to be supplied with a value of typeResolver.");
        //}


        #region Collection-type property creators

        public override ICPropPrivate<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PropertyChangedWithTValsEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;
            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>;

            ICPropPrivate<CT, T> prop = new CProp<CT, T>(initialValue, getDefaultValFunc, typeIsSolid, hasStorage,
                comparer, doWhenChangedX, doAfterNotify);
            return prop;
        }

        public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PropertyChangedWithTValsEventArgs<CT>> doWhenChangedX = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<CT>.Default.Equals;

            GetDefaultValueDelegate<CT> getDefaultValFunc = ValueConverter.GetDefaultValue<CT>; // this.GetDefaultValue<T>;

            ICPropPrivate<CT, T> prop = new CProp<CT, T>(getDefaultValFunc, typeIsSolid, hasStorage,
                comparer, doWhenChangedX, doAfterNotify);
            return prop;
        }

        #endregion

        #region Propety-type property creators

        // TODO: Need to create IPropPrivate<T> 
        public override IProp<T> Create<T>(
            T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; // this.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(initialValue, getDefaultValFunc, typeIsSolid: typeIsSolid,
                hasStore: hasStorage, comparer: comparer, doWhenChangedX: doWhenChangedX, doAfterNotify: doAfterNotify);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            EventHandler<PropertyChangedWithTValsEventArgs<T>> doWhenChangedX = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;

            GetDefaultValueDelegate<T> getDefaultValFunc = ValueConverter.GetDefaultValue<T>; // this.GetDefaultValue<T>;
            IProp<T> prop = new Prop<T>(getDefaultValFunc, typeIsSolid: typeIsSolid,
                hasStore: hasStorage, comparer: comparer, doWhenChangedX: doWhenChangedX,
                doAfterNotify: doAfterNotify);
            return prop;
        }

        #endregion

        #region Generic property creators

        public override IPropGen CreateGenFromObject(Type typeOfThisProperty,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PropertyChangedWithValsEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromObjectDelegate propCreator = GetPropCreator(typeOfThisProperty);
                IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else if (propKind == PropKindEnum.Collection)
            {
                CreateCPropFromObjectDelegate propCreator = GetCPropCreator(typeOfThisProperty, itemType);
                IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public override IPropGen CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PropertyChangedWithValsEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropFromStringDelegate propCreator = GetPropFromStringCreator(typeOfThisProperty);
                IPropGen prop = (IPropGen)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            } 
            else if(propKind == PropKindEnum.Collection)
            {
                CreateCPropFromStringDelegate propCreator = GetCPropFromStringCreator(typeOfThisProperty, itemType);
                IPropGen prop = (IPropGen)propCreator(this, value, useDefault, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        public override IPropGen CreateGenWithNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            EventHandler<PropertyChangedWithValsEventArgs> doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            if (propKind == PropKindEnum.Prop)
            {
                CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
                IPropGen prop = (IPropGen)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else if (propKind == PropKindEnum.Collection)
            {
                CreateCPropWithNoValueDelegate propCreator = GetCPropWithNoValueCreator(typeOfThisProperty, itemType);
                IPropGen prop = (IPropGen)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                    doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);
                return prop;
            }
            else
            {
                throw new InvalidOperationException($"PropKind = {propKind} is not recognized or is not supported.");
            }
        }

        #endregion

    }

}
