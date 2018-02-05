using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    public abstract class PropTemplate_NotUsed : IPropTemplate
    {
        #region Public Members

        public PropKindEnum PropKind { get; protected set; }
        public Type Type { get; }
        public bool TypeIsSolid { get; set; }
        public PropStorageStrategyEnum StorageStrategy { get; protected set; }

        public virtual Attribute[] Attributes { get; }

        public abstract bool ReturnDefaultForUndefined { get; }

        public event EventHandler<EventArgs> ValueChanged;

        #endregion

        #region Constructors

        public PropTemplate_NotUsed(PropKindEnum propKind, Type typeOfThisValue, bool typeIsSolid, PropStorageStrategyEnum storageStrategy)
        {
            PropKind = propKind;
            Type = typeOfThisValue;
            TypeIsSolid = typeIsSolid;
            StorageStrategy = storageStrategy;
            Attributes = new Attribute[] { };
        }

        #endregion

        #region Public Methods 

        //public abstract ValPlusType GetValuePlusType();
        //public abstract bool SetValueToUndefined();
        //public abstract void CleanUpTyped();
        //public abstract object Clone();

        //public abstract bool RegisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo);
        //public abstract bool UnregisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo);

        #endregion

        #region Private Methods

        // TODO: Make these extension methods for the PropKindEnum type.
        private bool IsThisACollection(PropKindEnum propKind)
        {
            if (propKind == PropKindEnum.Prop)
            {
                return false;
            }
            else if
                (
                propKind == PropKindEnum.ObservableCollection ||
                propKind == PropKindEnum.EnumerableTyped ||
                propKind == PropKindEnum.Enumerable ||
                propKind == PropKindEnum.ObservableCollection_RO ||
                propKind == PropKindEnum.EnumerableTyped_RO ||
                propKind == PropKindEnum.Enumerable_RO
                )
            {
                return true;
            }
            else
            {
                CheckPropKindSpecial(propKind);
                return false;
            }
        }

        private bool IsThisReadOnly(PropKindEnum propKind)
        {
            if (propKind == PropKindEnum.Prop)
            {
                return false;
            }
            else if
                (
                propKind == PropKindEnum.CollectionViewSource_RO ||
                propKind == PropKindEnum.ObservableCollection_RO ||
                propKind == PropKindEnum.EnumerableTyped_RO ||
                propKind == PropKindEnum.Enumerable_RO
                )
            {
                return true;
            }
            else
            {
                CheckPropKind(propKind);
                return false;
            }
        }

        #endregion

        #region Methods to Raise Events

        protected void OnValueChanged()
        {
            Interlocked.CompareExchange(ref ValueChanged, null, null)?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region DEBUG Checks

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckPropKind(PropKindEnum propKind)
        {
            if (!
                    (
                    propKind == PropKindEnum.CollectionViewSource ||
                    propKind == PropKindEnum.CollectionViewSource_RO ||
                    propKind == PropKindEnum.DataTable ||
                    propKind == PropKindEnum.DataTable_RO ||
                    propKind == PropKindEnum.Prop ||
                    propKind == PropKindEnum.ObservableCollection ||
                    //propKind == PropKindEnum.ObservableCollectionFB ||
                    propKind == PropKindEnum.EnumerableTyped ||
                    propKind == PropKindEnum.Enumerable ||
                    propKind == PropKindEnum.ObservableCollection_RO ||
                    //propKind == PropKindEnum.ObservableCollectionFB_RO ||
                    propKind == PropKindEnum.EnumerableTyped_RO ||
                    propKind == PropKindEnum.Enumerable_RO
                    )
                )
            {
                throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckPropKindSpecial(PropKindEnum propKind)
        {
            if (!
                    (
                    propKind == PropKindEnum.CollectionViewSource ||
                    propKind == PropKindEnum.CollectionViewSource_RO ||
                    propKind == PropKindEnum.DataTable ||
                    propKind == PropKindEnum.DataTable_RO
                    )
                )
            {
                throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
            }
        }

        #endregion 
    }
}
