using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
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

        //public List<PropertyChangedWithValsHandler> PropChangedWithValsHandlerList { get; set; }

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

        // Constructor
        public PropGenBase(Type typeOfThisValue, bool typeIsSolid, bool hasStore = true)
        {
            Type = typeOfThisValue;
            TypedProp = null;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;

            //PropChangedWithValsHandlerList = new List<PropertyChangedWithValsHandler>();
        }


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

        public event PropertyChangedWithValsHandler PropertyChangedWithVals;

        public void OnPropertyChangedWithVals(string propertyName, object oldVal, object newVal)
        {
            PropertyChangedWithValsHandler handler = Interlocked.CompareExchange(ref PropertyChangedWithVals, null, null);

            if (handler != null)
                handler(this, new PropertyChangedWithValsEventArgs(propertyName, oldVal, newVal));
        }

        #endregion

        #region Helper Methods for the Generic Method Templates

        // Delegate declarations.
        private delegate object GetPropValDelegate(object prop);

        private static GetPropValDelegate GetPropGetter(Type typeOfThisValue)
        {
            MethodInfo methInfoGetProp = GMT_TYPE.GetMethod("GetPropValue", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeOfThisValue);
            GetPropValDelegate result = (GetPropValDelegate)Delegate.CreateDelegate(typeof(GetPropValDelegate), methInfoGetProp);

            return result;
        }

        #endregion

        ~PropGenBase()
        {
            PropertyChangedWithVals = null;
        }

        #region Generic Method Templates

        static class GenericMethodTemplates
        {
            private static object GetPropValue<T>(object prop)
            {
                return ((IProp<T>)prop).TypedValue;
            }
        }

        #endregion

    }
}
