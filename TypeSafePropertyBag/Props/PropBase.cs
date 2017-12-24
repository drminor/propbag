using System;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using IRegisterBindingsFowarderType = IRegisterBindingsForwarder<UInt32>;

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

        DataSourceProvider _dataSourceProvider;
        public virtual DataSourceProvider DataSourceProvider
        {
            get
            {
                if(_dataSourceProvider == null)
                {
                    _dataSourceProvider = GetDataSourceProvider(PropKind);
                }
                return _dataSourceProvider;
            }
        }

        public virtual bool IsCollection() => IsThisACollection(PropKind);

        public virtual bool IsReadOnly() => IsThisReadOnly(PropKind);

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

        public abstract bool RegisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo);
        public abstract bool UnregisterBinding(IRegisterBindingsFowarderType forwarder, PropIdType propId, LocalBindingInfo bindingInfo);

        #endregion

        #region Private Methods

        private DataSourceProvider GetDataSourceProvider(PropKindEnum propKind)
        {
            DataSourceProvider result = null;
            return result;
        }

        private bool IsThisACollection(PropKindEnum propKind)
        {
            if (propKind == PropKindEnum.Prop)
            {
                return false;
            }
            else if
                (
                propKind == PropKindEnum.ObservableCollection ||
                propKind == PropKindEnum.ObservableCollectionFB ||
                propKind == PropKindEnum.EnumerableTyped ||
                propKind == PropKindEnum.Enumerable ||
                propKind == PropKindEnum.ObservableCollection_RO ||
                propKind == PropKindEnum.ObservableCollectionFB_RO ||
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
                propKind == PropKindEnum.ObservableCollectionFB_RO ||
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
                    propKind == PropKindEnum.ObservableCollectionFB ||
                    propKind == PropKindEnum.EnumerableTyped ||
                    propKind == PropKindEnum.Enumerable ||
                    propKind == PropKindEnum.ObservableCollection_RO ||
                    propKind == PropKindEnum.ObservableCollectionFB_RO ||
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
