using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public class PropFactory : AbstractPropFactory
    {
        public override IProp<T> Create<T>(
            T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            IProp<T> prop = new Prop<T>(initialValue, hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {

            IProp<T> prop = new Prop<T>(hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid)
        {
            CreatePropWithValueDelegate propCreator = GetPropCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid);
            return prop;
        }

        public override IPropGen CreateNoValue(Type typeOfThisProperty, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid)
        {
            CreatePropDelegate propCreator = GetPropWithNoneOrDefaultCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid);
            return prop;
        }

        public override IPropGen CreateFull(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer)
        {
            CreateFullPropWithValueDelegate propCreator = GetFullPropCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid, 
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer);
            return prop;
        }

        public override IPropGen CreateFullNoValue(Type typeOfThisProperty, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer)
        {
            CreateFullPropDelegate propCreator = GetFullPropWithNoneOrDefaultCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid,
                doWhenChanged: doWhenChanged, doAfterNotify: doAfterNotify, comparer: comparer);
            return prop;
        }

    }

}
