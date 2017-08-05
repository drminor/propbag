using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace DRM.PropBag
{
    public abstract class AbstractPropFactory
    {
        public abstract IProp<T> Create<T>(T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null);

        public abstract IProp<T> CreateWithNoneOrDefault<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null);

        public abstract IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid);
    
        
        public virtual IPropGen CreatePropInferType(object value, string propertyName, object extraInfo, bool hasStorage)
        {
            System.Type typeOfThisValue;
            bool typeIsSolid;

            if (value == null)
            {
                typeOfThisValue = typeof(object);
                typeIsSolid = false;
            }
            else
            {
                typeOfThisValue = value.GetType();
                typeIsSolid = true;
            }

            IPropGen prop = this.Create(typeOfThisValue, value, propertyName, extraInfo, hasStorage, typeIsSolid);
            return prop;
        }


        #region Shared Delegate Creation Logic

        static private Type gmtType = typeof(APFGenericMethodTemplates);

        protected virtual CreatePropDelegate GetPropCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreateProp", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropDelegate result = (CreatePropDelegate)Delegate.CreateDelegate(typeof(CreatePropDelegate), mi);

            return result;
        }

        protected virtual CreatePropWithNoneOrDefaultDelegate GetPropWithNoneOrDefaultCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropWithNoneOrDefault", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropWithNoneOrDefaultDelegate result = (CreatePropWithNoneOrDefaultDelegate)Delegate.CreateDelegate(typeof(CreatePropWithNoneOrDefaultDelegate), mi);

            return result;
        }

        #endregion
    }

    public class PropFactory : AbstractPropFactory
    {
        public override IProp<T> Create<T>(T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            IProp<T> prop = new Prop<T>(initialValue, hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoneOrDefault<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            T initialVal = default(T);
            IProp<T> prop = new Prop<T>(initialVal, hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid)
        {
            CreatePropDelegate propCreator = GetPropCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid);
            return prop;
        }

    }

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
            return CreateWithNoneOrDefault(propertyName, extraInfo, dummy, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
        }

        public override IProp<T> CreateWithNoneOrDefault<T>(
            string propertyName, object extraInfo = null,
            bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            PropExternStore<T> propWithExtStore = new PropExternStore<T>(propertyName, extraInfo, typeIsSolid, doWhenChanged, doAfterNotify, comparer);

            //    public delegate T GetExtVal<T>(Guid tag);

            //GetExtVal<T> xx = ((x) => x.ToString());

            //propWithExtStore.Getter = xx;
            return propWithExtStore;
        }


        public override IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo, bool hasStorage, bool isTypeSolid)
        {
            CreatePropDelegate propCreator = GetPropCreator(typeOfThisProperty);
            IPropGen prop = (IPropGen)propCreator(this, value, propertyName, extraInfo, hasStorage: true, isTypeSolid: isTypeSolid);
            return prop;
        }

    }

    static class APFGenericMethodTemplates
    {
        private static IProp<T> CreateProp<T>(AbstractPropFactory propFactory, object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid)
        {
            //PropFactory pf = propFactory as PropFactory;
            return propFactory.Create<T>((T)value, propertyName, extraInfo, hasStorage, isTypeSolid);
        }

        public static IProp<T> CreatePropWithNoneOrDefault<T>(AbstractPropFactory propFactory,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid)
        {
            //PropFactory pf = propFactory as PropFactory;
            return propFactory.CreateWithNoneOrDefault<T>(propertyName, extraInfo, hasStorage, isTypeSolid);
        }
    }


}
