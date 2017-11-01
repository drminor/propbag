using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class PropExtStoreFactory : AbstractPropFactory
    {
        object Stuff { get; }

        public override bool ProvidesStorage
        {
            get { return false; }
        }

        #region Constructors

        public PropExtStoreFactory(bool returnDefaultForUndefined)
            : this(null, returnDefaultForUndefined)
        {
        }

        public PropExtStoreFactory(object stuff, bool returnDefaultForUndefined,
            ResolveTypeDelegate typeResolver = null,
            IConvertValues valueConverter = null)
            : base(returnDefaultForUndefined, typeResolver, valueConverter)
        {
            // Info to help us set up the getters and setters
            Stuff = stuff;
        }

        #endregion

        #region Collection-type property creators

        public override ICPropPrivate<CT, T> Create<CT, T>(
            CT initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<CT, CT> doWhenChanged = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            ICPropPrivate<CT, T> prop = null;
            return prop;
        }

        public override ICPropPrivate<CT, T> CreateWithNoValue<CT, T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<CT, CT> doWhenChanged = null, bool doAfterNotify = false, Func<CT, CT, bool> comparer = null)
        {
            ICPropPrivate<CT, T> prop = null;
            return prop;
        }

        #endregion

        #region Propety-type property creators

        public override IProp<T> Create<T>(T initialValue,
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with inital values.");

            //return CreateWithNoValue(propertyName, extraInfo, dummy, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
        }

        public override IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;
            GetDefaultValueDelegate<T> getDefaultValFunc = this.GetDefaultValue<T>;

            PropExternStore<T> propWithExtStore = new PropExternStore<T>(propertyName, extraInfo, getDefaultValFunc, typeIsSolid: typeIsSolid, comparer: comparer, doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify);

            return propWithExtStore;
        }

        #endregion

        #region Generic Prop Creators

        public override IPropGen CreateGenFromObject(Type typeOfThisProperty,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with inital values.");
        }

        public override IPropGen CreateGenFromString(Type typeOfThisProperty,
            string value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            throw new InvalidOperationException("External Store Factory doesn't know how to create properties with inital values.");
        }

        public override IPropGen CreateGenWithNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid, PropKindEnum propKind,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false, Type itemType = null)
        {
            CreatePropWithNoValueDelegate propCreator = GetPropWithNoValueCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer, useRefEquality: useRefEquality);

            return prop;
        }

        #endregion
    }

}
