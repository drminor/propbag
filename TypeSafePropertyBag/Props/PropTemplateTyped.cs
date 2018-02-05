using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    public class PropTemplateTyped<T> : IPropTemplate<T>
    {
        #region Public and Protected Properties

        public event EventHandler<EventArgs> ValueChanged;

        public PropKindEnum PropKind { get; protected set; }
        public Type Type { get; }
        //public bool TypeIsSolid { get; set; }
        public PropStorageStrategyEnum StorageStrategy { get; protected set; }

        public virtual Attribute[] Attributes { get; }

        public Func<T, T, bool> Comparer { get; }
        public Func<string, T> GetDefaultValFunc { get; }

        public object ComparerProxy => Comparer;
        public object GetDefaultValFuncProxy => GetDefaultValFunc;

        #endregion

        #region Constructors

        public PropTemplateTyped(PropKindEnum propKind, PropStorageStrategyEnum storageStrategy,
            //bool typeIsSolid,
            Func<T, T, bool> comparer, Func<string, T> defaultValFunc)
        {
            PropKind = propKind;
            Type = typeof(T);
            //TypeIsSolid = typeIsSolid;
            StorageStrategy = storageStrategy;
            Attributes = new Attribute[] { };

            Comparer = comparer;
            GetDefaultValFunc = defaultValFunc;
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
         