using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

/// <remarks>
/// Copied from http://pelebyte.net/blog/2009/10/01/collections-collectionviews-and-a-wpf-binding-memory-leak/
/// </remarks>


namespace DRM.TypeSafePropertyBag.DataAccessSupport
{

    public class BetterObservableCollection_NotUsed<T> : ObservableCollection<T>, ICollectionViewFactory
    {
        public ICollectionView CreateView()
        {
            return new BetterListCollectionView(this);
        }
    }

    public class BetterListCollectionView :
        ListCollectionView, IWeakEventListener
    {
        public BetterListCollectionView(IList list) : base(list)
        {
            if (list is INotifyCollectionChanged changed)
            {
                // this fixes the problem
                changed.CollectionChanged -= this.OnCollectionChanged;
                CollectionChangedEventManager.AddListener(changed, this);
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (e is NotifyCollectionChangedEventArgs nccEventArgs)
            {
                OnCollectionChanged(nccEventArgs);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
