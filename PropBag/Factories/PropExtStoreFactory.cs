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

        public override IProp<T> Create<T>(T initialValue,
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            return CreateWithNoValue(propertyName, extraInfo, dummy, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
        }

        public override IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            PropExternStore<T> propWithExtStore = new PropExternStore<T>(propertyName, extraInfo, typeIsSolid, doWhenChanged, doAfterNotify, comparer);

            //public delegate T GetExtVal<T>(Guid tag);
            //GetExtVal<T> xx = ((x) => x.ToString());
            //propWithExtStore.Getter = xx;

            return propWithExtStore;
        }


        public override IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid)
        {
            CreatePropWithValueDelegate propCreator = GetPropCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid);
            return prop;
        }

    }

}
