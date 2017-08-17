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
        public abstract IProp<T> Create<T>(
            T initialValue,
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null);

        public abstract IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null);

        // TODO: Consider removing the "short" versions" and provide defaults for the "long" versions.
        public abstract IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid);

        public abstract IPropGen CreateNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid);

        public abstract IPropGen CreateFull(Type typeOfThisProperty,
            object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        public abstract IPropGen CreateFullNoValue(Type typeOfThisProperty,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);
        
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
                // TODO, we probably need to be more creative when determining the type of this new value.
                typeOfThisValue = value.GetType();
                typeIsSolid = true;
            }

            IPropGen prop = this.Create(typeOfThisValue, value, propertyName, extraInfo, hasStorage, typeIsSolid);
            return prop;
        }

        public virtual IEqualityComparer<T> GetRefEqualityComparer<T>()
        {
            return RefEqualityComparer<T>.Default;
        }


        #region Shared Delegate Creation Logic

        static private Type gmtType = typeof(APFGenericMethodTemplates);

        protected virtual CreatePropWithValueDelegate GetPropCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreateProp", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropWithValueDelegate result = (CreatePropWithValueDelegate)Delegate.CreateDelegate(typeof(CreatePropWithValueDelegate), mi);

            return result;
        }

        protected virtual CreatePropDelegate GetPropWithNoneOrDefaultCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropDelegate result = (CreatePropDelegate)Delegate.CreateDelegate(typeof(CreatePropDelegate), mi);

            return result;
        }

        protected virtual CreateFullPropWithValueDelegate GetFullPropCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreateFullProp", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreateFullPropWithValueDelegate result = (CreateFullPropWithValueDelegate)Delegate.CreateDelegate(typeof(CreateFullPropWithValueDelegate), mi);

            return result;
        }

        protected virtual  CreateFullPropDelegate GetFullPropWithNoneOrDefaultCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreateFullPropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreateFullPropDelegate result = (CreateFullPropDelegate)Delegate.CreateDelegate(typeof(CreateFullPropDelegate), mi);

            return result;
        }

        #endregion
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

        public static IProp<T> CreatePropWithNoValue<T>(AbstractPropFactory propFactory,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid)
        {
            //PropFactory pf = propFactory as PropFactory;
            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid);
        }

        private static IProp<T> CreateFullProp<T>(AbstractPropFactory propFactory, object value,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            IEqualityComparer<T> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (IEqualityComparer<T>)comparer;

            // TODO: Check This.
            T theVal = (T)value;

            return propFactory.Create<T>(theVal, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T,T>) doWhenChanged, doAfterNotify, compr);
        }

        public static IProp<T> CreateFullPropWithNoValue<T>(AbstractPropFactory propFactory,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            IEqualityComparer<T> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (IEqualityComparer<T>)comparer;

            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid, 
                (Action<T, T>)doWhenChanged, doAfterNotify, compr);
        }

    }


}
