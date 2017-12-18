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
    using PropIdType = UInt32;

    /// <summary>
    /// A wrapper for an instance of IProp<typeparam name="T"/>.
    /// </summary>
    public abstract class PropBase : IProp
    {
        #region Public Members

        public PropKindEnum PropKind { get; protected set; }
        public Type Type { get; }
        public bool TypeIsSolid { get; set; }
        public bool HasStore { get; protected set; }

        public virtual Attribute[] Attributes { get; }

        public bool ValueIsDefined { get; protected set; }
        public abstract object TypedValueAsObject { get; }
        public abstract IListSource ListSource { get; }

        #endregion

        #region Constructors

        public PropBase(PropKindEnum propKind, Type typeOfThisValue, bool typeIsSolid, bool hasStore, bool valueIsDefined)
        {
            PropKind = propKind;
            Type = typeOfThisValue;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;
            ValueIsDefined = valueIsDefined;
            Attributes = new Attribute[] { };
        }

        #endregion

        #region Public Methods 

        public abstract ValPlusType GetValuePlusType();
        public abstract bool SetValueToUndefined();
        public abstract void CleanUpTyped();
        public abstract object Clone();

        public abstract bool RegisterBinding(IPropBagInternal propBag, PropIdType propId, LocalBindingInfo bindingInfo);
        public abstract bool UnregisterBinding(IPropBagInternal propBag, PropIdType propId, LocalBindingInfo bindingInfo);

        #endregion
    }
}
