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
    /// <summary>
    /// A wrapper for an instance of IProp<typeparam name="T"/>.
    /// </summary>
    public abstract class PropBase : IProp
    {
        #region Public Members

        public PropKindEnum PropKind { get; private set; }
        public Type Type { get; }
        public bool TypeIsSolid { get; set; }
        public bool HasStore { get; private set; }

        Attribute[] _attributes;
        public virtual Attribute[] Attributes => _attributes;

        public abstract bool ValueIsDefined { get; }
        public abstract object TypedValueAsObject { get; }
        public abstract IListSource ListSource { get; }

        #endregion

        #region Constructors

        public PropBase(PropKindEnum propKind, Type typeOfThisValue, bool typeIsSolid, bool hasStore = true)
        {
            PropKind = propKind;
            Type = typeOfThisValue;
            TypeIsSolid = typeIsSolid;
            HasStore = hasStore;
            _attributes = new Attribute[] { };
        }

        #endregion

        #region Public Methods 

        public abstract ValPlusType GetValuePlusType();
        public abstract bool SetValueToUndefined();
        public abstract void CleanUpTyped();
        public abstract object Clone();

        #endregion
    }
}
