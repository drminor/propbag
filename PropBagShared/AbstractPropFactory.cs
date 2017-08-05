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
        public abstract IProp<T> Create<T>(T initialValue, bool hasStorage = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null);
        public abstract IProp<T> CreateWithNoneOrDefault<T>(bool hasStorage = true, bool typeIsSolid = true, Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null);

        public virtual IPropGen CreatePropInferType(object value)
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

            CreatePropDelegate propCreator = GetPropCreator(typeOfThisValue);
            IPropGen prop = (IPropGen) propCreator(this, value, hasStorage: true, isTypeSolid: typeIsSolid);

            return prop;
        }

        public virtual IPropGen Create(object value, Type type)
        {
            CreatePropDelegate propCreator = GetPropCreator(type);
            IPropGen prop = (IPropGen)propCreator(this, value, hasStorage: true, isTypeSolid: true);

            return prop;
        }

        private static Type gmtType = typeof(AbstractPropFactory.GenericMethodTemplates);

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

        static class GenericMethodTemplates
        {
            private static IProp<T> CreateProp<T>(AbstractPropFactory propFactory, object value, bool hasStorage, bool isTypeSolid)
            {
                return propFactory.Create<T>((T)value, hasStorage, isTypeSolid);
            }

            private static IProp<T> CreatePropWithNoneOrDefault<T>(AbstractPropFactory propFactory, bool hasStorage, bool isTypeSolid)
            {
                return propFactory.CreateWithNoneOrDefault<T>(hasStorage, isTypeSolid);
            }
        }

    }

    public class PropFactory : AbstractPropFactory
    {
        public override IProp<T> Create<T>(T initialValue, bool hasStorage = true, bool typeIsSolid = true, 
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            IProp<T> prop = new Prop<T>(initialValue, hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return prop;
        }

        public override IProp<T> CreateWithNoneOrDefault<T>(bool hasStorage = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            T initialVal = default(T);
            IProp<T> prop = new Prop<T>(initialVal, hasStorage, typeIsSolid, doWhenChanged, doAfterNotify, comparer);
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


        public override IProp<T> Create<T>(T initialValue, bool dummy = true, bool typeIsSolid = true, 
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            PropExternStore<T> propWithExtStore = new PropExternStore<T>(typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return propWithExtStore;
        }

        public override IProp<T> CreateWithNoneOrDefault<T>(bool dummy = true, bool typeIsSolid = true,
            Action<T, T> doWhenChanged = null, bool doAfterNotify = false, IEqualityComparer<T> comparer = null)
        {
            PropExternStore<T> propWithExtStore = new PropExternStore<T>(typeIsSolid, doWhenChanged, doAfterNotify, comparer);
            return propWithExtStore;
        }

    }
}
