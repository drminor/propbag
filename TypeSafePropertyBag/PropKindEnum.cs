
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

        ObservableCollectionFB,     // ICPropFB<CT, T> : IProp<CT> where CT : ObservableCollection<T>
        ObservableCollectionFB_RO,  // IReadOnlyCPropFB<CT, T> : IProp<CT> where CT : ReadOnlyObservableCollection<T>

        CollectionViewSource,       // ICViewProp<TCVS, T> : IReadOnlyCViewProp<TCVS, T> where TCVS: class
        CollectionViewSource_RO,    // IReadOnlyCViewProp<TCVS, T> : IProp<TCVS> where TCVS : class

        CollectionView,

        DataTable,
        DataTable_RO
    }
}
