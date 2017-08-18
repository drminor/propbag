using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class PropExtStoreFactory : AbstractPropFactory
    {
        object Stuff;
        public PropExtStoreFactory(object stuff)
        {
            // Info to help us set up the getters and setters
            Stuff = stuff;
        }

        // TODO: consider throwing not implemented, instead of passively throwing away the initial value.
        public override IProp<T> Create<T>(T initialValue,
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            return CreateWithNoValue(propertyName, extraInfo, dummy, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
        }

        public override IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default.Equals;
            PropExternStore<T> propWithExtStore = new PropExternStore<T>(propertyName, extraInfo, typeIsSolid, doWhenChanged, doAfterNotify, comparer);

            return propWithExtStore;
        }

        public override IPropGen CreateGen(Type typeOfThisProperty,
            object value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            CreatePropDelegate propCreator = GetPropCreator(typeOfThisProperty);
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
