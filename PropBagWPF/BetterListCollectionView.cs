using DRM.TypeSafePropertyBag;
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

namespace DRM.PropBagWPF
{
    public class BetterListCollectionViewCreator<T> where T: INotifyItemEndEdit
    {
        public static ICollectionView ProduceView(ObservableCollection<T> list)
        {
            ICollectionView result = new BetterListCollectionView<T>(list);

            return result;
        }
    }


    public class BetterListCollectionView<T> : ListCollectionView, IWeakEventListener
    {
        public BetterListCollectionView(ObservableCollection<T> list) : base(list)
        {
            //if (list is INotifyCollectionChanged changed)
            //{
            //    this fixes the problem
            //    changed.CollectionChanged -= this.OnCollectionChanged;
            //    CollectionChangedEventManager.AddListener(changed, this);
            //}

            list.CollectionChanged -= this.OnCollectionChanged;
            CollectionChangedEventManager.AddListener(list, this);
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

    //public class BetterObservableCollection_NotUsed<T> : ObservableCollection<T>, ICollectionViewFactory
    //{
    //    public ICollectionView CreateView()
    //    {
    //        return new BetterListCollectionView(this);
    //    }
    //}
}
