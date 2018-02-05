
using System;

namespace DRM.TypeSafePropertyBag
{
    public enum PropKindEnum
    {
        Prop,
        Enumerable,                 // IEProp<CT> : IProp<CT>, IReadOnlyEProp<CT> where CT : IEnumerable
        Enumerable_RO,              // IReadOnlyEProp<CT> : IProp<CT> where CT : IEnumerable

        EnumerableTyped,            // IETypedProp<CT, T> : IEProp<CT>, IReadOnlyETypedProp<CT, T> where CT : IEnumerable<T>
        EnumerableTyped_RO,         // IReadOnlyETypedProp<CT, T> : IReadOnlyEProp<CT> where CT : IEnumerable<T>

        ObservableCollection,       // ICProp<CT,T> : IETypedProp<CT, T>, IReadOnlyCProp<CT, T> where CT : IObsCollection<T>
        ObservableCollection_RO,    // IReadOnlyCProp<CT,T> : IReadOnlyETypedProp<CT, T> where CT : IReadOnlyObsCollection<T>

        //ObservableCollectionFB,     // ICPropFB<CT, T> : IProp<CT> where CT : ObservableCollection<T>
        //ObservableCollectionFB_RO,  // IReadOnlyCPropFB<CT, T> : IProp<CT> where CT : ReadOnlyObservableCollection<T>

        CollectionViewSource,       // ICViewProp<CVST, T> : IReadOnlyCViewProp<CVST, T> where CVST: class
        CollectionViewSource_RO,    // IReadOnlyCViewProp<CVST, T> : IProp<CVST> where CVST : class

        CollectionView,

        DataTable,
        DataTable_RO
    }

    public static class PropKindEnumExtensions
    {
        public static bool IsCollection(this PropKindEnum propKind)
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

        public static bool IsReadOnly(this PropKindEnum propKind)
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

        #region DEBUG Checks

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CheckPropKind(PropKindEnum propKind)
        {
            if (!
                    (
                    propKind == PropKindEnum.CollectionView ||
                    propKind == PropKindEnum.CollectionViewSource ||
                    propKind == PropKindEnum.CollectionViewSource_RO ||
                    propKind == PropKindEnum.DataTable ||
                    propKind == PropKindEnum.DataTable_RO ||
                    propKind == PropKindEnum.Prop ||
                    propKind == PropKindEnum.ObservableCollection ||
                    propKind == PropKindEnum.EnumerableTyped ||
                    propKind == PropKindEnum.Enumerable ||
                    propKind == PropKindEnum.ObservableCollection_RO ||
                    propKind == PropKindEnum.EnumerableTyped_RO ||
                    propKind == PropKindEnum.Enumerable_RO
                    )
                )
            {
                throw new InvalidOperationException($"The PropKind: {propKind} is not supported.");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CheckPropKindSpecial(PropKindEnum propKind)
        {
            if (!
                    (
                    propKind == PropKindEnum.CollectionView ||
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
