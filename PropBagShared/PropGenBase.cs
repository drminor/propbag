using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using DRM.Ipnwvc;

namespace DRM.PropBag
{
    public abstract class PropGenBase : IPropGen
    {
        static private Type GMT_TYPE = typeof(GenericMethodTemplates);

        public Type Type {get; set;}

        /// <summary>
        /// An instance of IProp<typeparam name="T"/>.
        /// Our callers could simply cast all instances that inherit from PropGenBase into a IProp<typeparamref name="T"/>
        /// When they need to access the instanace as a IProp<typeparamref name="T"/>, but this makes it more formal.
        /// </summary>
        public object TypedProp { get; set; }

        public bool TypeIsSolid { get; set; }

        public bool HasStore { get; set; }

        public List<PropertyChangedWithValsHandler> PropChangedWithValsHandlerList { get; set; }


        private GetPropValDelegate _doGet;
        private GetPropValDelegate DoGetProVal
        {
            get
            {
                if (_doGet == null)
                {
                    _doGet = GetPropGetter(Type);
                }
                return _doGet;
            }

            set
            {
                _doGet = value;
            }
        }

        //public DoSetDelegate DoSetProVal { get; set; }


        // Constructor
        public PropGenBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore = true)
        {
            Type = typeOfThisValue;
            TypedProp = null;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;


            PropChangedWithValsHandlerList = new List<PropertyChangedWithValsHandler>();
            //DoGetProVal = null;
            //DoSetProVal = null;
        }

        #region Factory Methods

        //static public IPropGen CreateValueInferType(object value)
        //{
        //    System.Type typeOfThisValue;
        //    bool typeIsSolid;

        //    if (value == null)
        //    {
        //        typeOfThisValue = typeof(object);
        //        typeIsSolid = false;
        //    }
        //    else
        //    {
        //        typeOfThisValue = value.GetType();
        //        typeIsSolid = true;
        //    }

        //    CreateVWTDelegate createValWithType = GetVWTCreator(typeOfThisValue);
        //    IPropGen vwt = createValWithType(value, typeIsSolid);

        //    return vwt;
        //}

        #endregion

        #region Public Methods and Properties

        /// <summary>
        /// Uses Reflection
        /// </summary>
        public object Value
        {
            get
            {
                return DoGetProVal(TypedProp);
            }
        }

        //public void UpdateWithSolidType(Type typeOfThisValue, object curValue)
        //{
        //    // Create a brand new ValueWithType instance -- we could have created a custom Delegate for this -- but this strategy reuses more code.
        //    CreateVWTDelegate vwtCreator = GetVWTCreator(typeOfThisValue);
        //    IPropGen newVwt = vwtCreator(curValue, true);
            
        //    //this.Prop = newVwt.Prop;
        //    this.Type = typeOfThisValue;
        //    this.TypeIsSolid = true;
        //    this.HasStore = newVwt.HasStore;

        //    // Clear the cached getter and setter delegates, since these depend on the type of the value.
        //    //this.DoGetProVal = null;
        //    this.DoSetProVal = null;

        //    // Do not update the members of the PropChangedWithValsHandlerList, since these delegates use object references, not typed references.
        //}

        #endregion

        //public bool UpdateDoWhenChanged<T>(Action<T, T> doWhenChanged, bool doAfterNotify)
        //{
        //    //IProp<T> prop = (IProp<T>) this.Prop;
        //    IProp<T> prop = (IProp<T>) this;

        //    bool hadExistingValue = prop.DoWHenChangedAction != null;

        //    prop.DoWHenChangedAction = doWhenChanged;
        //    prop.DoAfterNotify = doAfterNotify;

        //    return hadExistingValue;
        //}

        #region Delegate declarations

        // Delegate declarations.
        private delegate object GetPropValDelegate(object prop);
        //private delegate IPropGen CreateVWTDelegate(object value, bool typeIsSolid);

        #endregion

        #region Helper Methods for the Generic Method Templates

        private static GetPropValDelegate GetPropGetter(Type typeOfThisValue)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetPropValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            GetPropValDelegate result = (GetPropValDelegate)Delegate.CreateDelegate(typeof(GetPropValDelegate), methInfoGetProp);

            return result;
        }

        //private static CreateVWTDelegate GetVWTCreator(Type typeOfThisValue)
        //{
        //    MethodInfo methInfoVWTCreator = GMT_TYPE.GetMethod("CreateVWT", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
        //    CreateVWTDelegate result = (CreateVWTDelegate)Delegate.CreateDelegate(typeof(CreateVWTDelegate), methInfoVWTCreator);

        //    return result;
        //}

        #endregion

        #region Generic Method Templates

        static class GenericMethodTemplates
        {
            private static object GetPropValue<T>(object prop)
            {
                return ((IProp<T>)prop).TypedValue;
            }

            //private static IPropGen CreateVWT<T>(object value, bool isTypeSolid)
            //{
            //    return IPropGen.Create<T>((T)value, null, false, null, isTypeSolid);
            //}
        }

        #endregion

    }
}
