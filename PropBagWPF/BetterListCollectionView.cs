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
            BetterListCollectionView<T> result = new BetterListCollectionView<T>(list);
            return result;
        }
    }

    public class BetterListCollectionView<T> : ListCollectionView, IWeakEventListener
    {
        public BetterListCollectionView(ObservableCollection<T> list) : base(list)
        {
            if (list is INotifyCollectionChanged collectionWithIncc)
            {
                // Remove the subscription just created by our base class: ListCollectionView::CollectionView
                // and replace with Weak Subscription.
                // When we receive a WeakEvent, call our base class' OnCollectionChanged.
                collectionWithIncc.CollectionChanged -= this.OnCollectionChanged;
                CollectionChangedEventManager.AddListener(collectionWithIncc, this);
            }

            //list.CollectionChanged -= this.OnCollectionChanged;
            //CollectionChangedEventManager.AddListener(list, this);
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

        //NotifyCollectionChangedEventHandler _backingStore;
        //protected override event NotifyCollectionChangedEventHandler CollectionChanged
        //{
        //    add
        //    {
        //        _backingStore += value;
        //        int cnt = InspectInvocationList(_backingStore);
        //        //INotifyCollectionChanged ccSource = InternalList as INotifyCollectionChanged;

        //        //if(InternalList != null && ccSource == null)
        //        //{
        //        //    throw new InvalidOperationException("The internal list does not implement INotifyCollectionChanged.");
        //        //}

        //        //CollectionChangedEventManager.AddListener(ccSource, this);
        //    }
        //    remove
        //    {
        //        _backingStore -= value;
        //        int cnt = InspectInvocationList(_backingStore);

        //        //INotifyCollectionChanged ccSource = InternalList as INotifyCollectionChanged;

        //        //if (InternalList != null && ccSource == null)
        //        //{
        //        //    throw new InvalidOperationException("The internal list does not implement INotifyCollectionChanged.");
        //        //}

        //        //CollectionChangedEventManager.RemoveListener(ccSource, this);
        //    }
        //}

        protected override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                base.CollectionChanged += value; // _collectionChanged += value;
            }
            remove
            {
                base.CollectionChanged -= value; // _collectionChanged -= value;
            }
        }

        private int InspectInvocationList(NotifyCollectionChangedEventHandler cc)
        {
            MulticastDelegate m = (MulticastDelegate)cc;

            var list = m.GetInvocationList();

            foreach (Delegate d in list)
            {
                System.Diagnostics.Debug.WriteLine(d.Target);
                System.Diagnostics.Debug.WriteLine(d.Method);
            }
            return list.Length;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            base.OnCollectionChanged(args);
        }
    }
}
