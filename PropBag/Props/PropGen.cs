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
using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.PropBag
{
    public struct PropGen : IPropGen
    {
        public SimpleExKey PropId { get; }

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

        // Constructors
        public PropGen(IProp genericTypedProp, SimpleExKey propId)
        {
            TypedProp = genericTypedProp ?? throw new ArgumentNullException($"{nameof(genericTypedProp)} must be non-null.");
            PropId = propId;
            IsEmpty = false;


            //PropertyChangedWithGenVals = null; // = delegate { };
            //PropertyChanged = null;
            //_actTable = null;
        }

        public PropGen(bool? makeEmpty)
        {
            TypedProp = null;
            PropId = new SimpleExKey();
            IsEmpty = true;
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
