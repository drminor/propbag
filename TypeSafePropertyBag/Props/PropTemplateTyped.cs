using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    public class PropTemplateTyped<T> : IPropTemplate<T>
    {
        #region Public and Protected Properties

        public PropKindEnum PropKind { get; protected set; }
        public Type Type { get; }
        public bool TypeIsSolid { get; set; }
        public PropStorageStrategyEnum StorageStrategy { get; protected set; }

        public virtual Attribute[] Attributes { get; }

        public bool ReturnDefaultForUndefined => GetDefaultValFunc != null;

        public event EventHandler<EventArgs> ValueChanged;

        public Func<T, T, bool> Comparer { get; }
        public GetDefaultValueDelegate<T> GetDefaultValFunc { get; }

        #endregion

        #region Constructors

        public PropTemplateTyped(PropKindEnum propKind, PropStorageStrategyEnum storageStrategy,
            bool typeIsSolid, Func<T, T, bool> comparer, GetDefaultValueDelegate<T> defaultValFunc)
        {
            PropKind = propKind;
            Type = typeof(T);
            TypeIsSolid = typeIsSolid;
            StorageStrategy = storageStrategy;
            Attributes = new Attribute[] { };

            Comparer = comparer;
            GetDefaultValFunc = defaultValFunc;
        }

        #endregion

        #region Public Methods

        public bool Compare(T val1, T val2)
        {
            //if (!ValueIsDefined) return false;

            return Comparer(val1, val2);
        }

        #endregion

        #region Methods to Raise Events

        protected void OnValueChanged()
        {
            Interlocked.CompareExchange(ref ValueChanged, null, null)?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
         