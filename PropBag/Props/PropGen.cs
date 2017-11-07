using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Reflection;

using System.Runtime.CompilerServices;
using DRM.TypeSafePropertyBag;
using System.ComponentModel;
using System.Windows;

namespace DRM.PropBag
{
    public struct PropGen : IPropGen
    {
        public ulong PropId { get; }

        public IProp TypedProp { get; }

        public bool IsEmpty { get; }

        /// <summary>
        /// Does not use reflection.
        /// </summary>
        public object Value
        {
            get
            {
                return TypedProp.TypedValueAsObject;
                //return DoGetProVal(TypedProp);
            }
        }

        public ValPlusType ValuePlusType()
        {
            return new ValPlusType(Value, TypedProp.Type);
        }

        // Constructor
        public PropGen(IProp genericTypedProp, ulong? propId)
        {
            if(genericTypedProp == null)
            {
                TypedProp = null;
                PropId = 0;
                IsEmpty = true;
            }
            else
            {
                TypedProp = genericTypedProp;
                PropId = propId.Value;
                IsEmpty = false;
            }

            //PropertyChangedWithGenVals = null; // = delegate { };
            //PropertyChanged = null;
            //_actTable = null;
        }


        public void CleanUp(bool doTypedCleanup)
        {
            if(doTypedCleanup && TypedProp != null) TypedProp.CleanUpTyped();
            //_actTable = null;
            //PropertyChangedWithGenVals = null;
        }

        //public void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChangedEventHandler handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

        //    if (handler != null)
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //}

        //public void SubscribeToPropChanged(EventHandler<PropertyChangedEventArgs> handler)
        //{
        //    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(this, "PropertyChanged", handler);
        //}
    }
}
