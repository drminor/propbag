using DRM.TypeSafePropertyBag;
using System;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.PropBag.Collections
{
    public class CViewProp : PropTypedBase<ListCollectionView>, IProp<ListCollectionView> 
    {

        public CViewProp(ListCollectionView initialValue)
            : base(typeof(ListCollectionView), true, true, true, RefEqualityComparer<ListCollectionView>.Default.Equals, null, PropKindEnum.Collection)
        {
            TypedValue = initialValue;
        }

        override public IListSource ListSource => throw new NotSupportedException($"Prop Items implementing the ICViewProp<CT, T> interface do not provide a ListSource.");

        override public bool SetValueToUndefined() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot have their value set to undefined.");

        public override object Clone() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot be cloned.");

        public override void CleanUpTyped()
        {
            // TODO: Dispose of our items.
        }

        //public ReadOnlyObservableCollection<T> ReadOnlyObservableCollection
        //{
        //    get
        //    {
        //        return _sourceList.ReadOnlyObservableCollection;
        //    }
        //}

        //public static ListCollectionView GetEmptyView(string propertyName)
        //{
        //    ObservableCollection<T> temp = new ObservableCollection<T>();
        //    ListCollectionView result = GetDefaultView(temp);
        //    return result;
        //}

        //private static ListCollectionView GetDefaultView(ObservableCollection<T> collection)
        //{
        //    ICollectionView temp2 = CollectionViewSource.GetDefaultView(collection);
        //    ListCollectionView result = temp2 as ListCollectionView;
        //    return result;
        //}

    }
}
         