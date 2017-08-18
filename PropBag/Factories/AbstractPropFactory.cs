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
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null);

        public abstract IProp<T> CreateWithNoValue<T>(
            string propertyName, object extraInfo = null,
            bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, Func<T,T,bool> comparer = null);

        //// TODO: Consider removing the "short" versions" and provide defaults for the "long" versions.
        //public abstract IPropGen Create(Type typeOfThisProperty, object value, string propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid);

        //public abstract IPropGen CreateNoValue(Type typeOfThisProperty,
        //    string propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid);

        public abstract IPropGen CreateGen(Type typeOfThisProperty,
            object value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false);

        public abstract IPropGen CreateGenWithNoValue(Type typeOfThisProperty,
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

            IPropGen prop = this.CreateGen(typeOfThisValue, value, false, propertyName, extraInfo, hasStorage, typeIsSolid, null, false, null, false);
            return prop;
        }

        public virtual Func<T,T,bool> GetRefEqualityComparer<T>()
        {
            var y = RefEqualityComparer<T>.Default;

            DRM.PropBag.RefEqualityComparer<T> x = RefEqualityComparer<T>.Default;

            Func<T, T, bool> result = x.Equals;
            return x.Equals; // result;
            //return RefEqualityComparer<T>.Default;
        }

        public virtual T GetValue<T>(object value, bool useDefault)
        {
            if (useDefault) return default(T);

            // TODO: Check This.
            return (T) value;
        }


        #region Shared Delegate Creation Logic

        static private Type gmtType = typeof(APFGenericMethodTemplates);

        //protected virtual CreatePropWithValueDelegate GetPropCreator(Type typeOfThisValue)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreateProp", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        //    CreatePropWithValueDelegate result = (CreatePropWithValueDelegate)Delegate.CreateDelegate(typeof(CreatePropWithValueDelegate), mi);

        //    return result;
        //}

        //protected virtual CreatePropDelegate GetPropWithNoneOrDefaultCreator(Type typeOfThisValue)
        //{
        //    MethodInfo mi = gmtType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        //    CreatePropDelegate result = (CreatePropDelegate)Delegate.CreateDelegate(typeof(CreatePropDelegate), mi);

        //    return result;
        //}

        protected virtual CreatePropDelegate GetPropCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreateProp", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropDelegate result = (CreatePropDelegate)Delegate.CreateDelegate(typeof(CreatePropDelegate), mi);

            return result;
        }

        protected virtual  CreatePropWithNoValueDelegate GetPropWithNoValueCreator(Type typeOfThisValue)
        {
            MethodInfo mi = gmtType.GetMethod("CreatePropWithNoValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            CreatePropWithNoValueDelegate result = (CreatePropWithNoValueDelegate)Delegate.CreateDelegate(typeof(CreatePropWithNoValueDelegate), mi);

            return result;
        }

        #endregion
    }

    static class APFGenericMethodTemplates
    {
        //private static IProp<T> CreateProp<T>(AbstractPropFactory propFactory, object value,
        //    string propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid)
        //{
        //    //PropFactory pf = propFactory as PropFactory;
        //    return propFactory.Create<T>((T)value, propertyName, extraInfo, hasStorage, isTypeSolid);
        //}

        //public static IProp<T> CreatePropWithNoValue<T>(AbstractPropFactory propFactory,
        //    string propertyName, object extraInfo,
        //    bool hasStorage, bool isTypeSolid)
        //{
        //    //PropFactory pf = propFactory as PropFactory;
        //    return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid);
        //}

        private static IProp<T> CreateProp<T>(AbstractPropFactory propFactory,
            object value, bool useDefault,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            Func<T,T,bool> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (Func<T,T,bool>)comparer;

            T theVal = propFactory.GetValue<T>(value, useDefault);

            return propFactory.Create<T>(theVal, propertyName, extraInfo, hasStorage, isTypeSolid,
                (Action<T,T>) doWhenChanged, doAfterNotify, compr);
        }

        public static IProp<T> CreatePropWithNoValue<T>(AbstractPropFactory propFactory,
            string propertyName, object extraInfo,
            bool hasStorage, bool isTypeSolid,
            Delegate doWhenChanged, bool doAfterNotify, Delegate comparer, bool useRefEquality = false)
        {
            Func<T,T,bool> compr = useRefEquality ? propFactory.GetRefEqualityComparer<T>() : (Func<T,T,bool>)comparer;

            return propFactory.CreateWithNoValue<T>(propertyName, extraInfo, hasStorage, isTypeSolid, 
                (Action<T, T>)doWhenChanged, doAfterNotify, compr);
        }

    }


}
