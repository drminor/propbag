using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public class EndEditWrapper<T> : ObservableCollection<T>, ICollectionViewFactory, INotifyItemEndEdit, IDisposable
        where T: INotifyItemEndEdit
    {
        private BetterLCVCreatorDelegate<T> _betterListCollViewCreator;
        private ICollectionView _listWrapper;

        #region Constructors

        public EndEditWrapper()
        {
            throw new NotSupportedException("Constructing an EndEditWrapper with no parameters is not supported.");
        }

        // TODO: Do we need this? We already take an IEnumerable<T> and IList<T> implements IEnumerable<T>
        //public EndEditWrapper(List<T> list)
        //{
        //    foreach(T item in list)
        //    {
        //        // This calls insert and thereby we attach our handler to each item's ItemEndEdit event.
        //        Add(item);
        //    }
        //}

        public EndEditWrapper(IEnumerable<T> collection, BetterLCVCreatorDelegate<T> betterLCVCreatorDelegate)
        {
            _betterListCollViewCreator = betterLCVCreatorDelegate;

            foreach (T item in collection)
            {
                // This calls insert and thereby we attach our handler to each item's ItemEndEdit event.
                Add(item);
            }
        }

        #endregion

        #region ICollectionViewFactory Implementation

        public ICollectionView CreateView()
        {
            //// Unsubscribe our handler from the CollectionView, if any.
            //if(_listWrapper != null)
            //{
            //    _listWrapper.CollectionChanged -= _listWrapper_CollectionChanged;
            //}

            // Create the new CollectionView using the provided delegate.
            _listWrapper = _betterListCollViewCreator(this);

            //// Subscribe to the new CollectionView, if not null.
            //if(_listWrapper != null)
            //{
            //    _listWrapper.CollectionChanged += _listWrapper_CollectionChanged;
            //}

            return _listWrapper;
        }

        //private void _listWrapper_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    OnCollectionChanged(e);
        //}

        #endregion

        #region ObservableCollection<T> overrides

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            // handle any EndEdit events relating to this item
            // XXTemp
            item.ItemEndEdit += ItemEndEditHandler;
        }

        protected override void ClearItems()
        {
            foreach (object o in Items)
            {
                if (o is IDisposable disable)
                {
                    disable.Dispose();
                }
            }

            base.ClearItems();
        }

        #endregion

        #region Event Declartions and Helpers

        public event EventHandler<EventArgs> ItemEndEdit;

        //NotifyCollectionChangedEventHandler _collectionChanged;
        public override event NotifyCollectionChangedEventHandler CollectionChanged
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

        void ItemEndEditHandler(object sender, EventArgs e)
        {
            // simply forward any EndEdit events
            ItemEndEdit?.Invoke(sender, e);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    //if(_listWrapper != null)
                    //{
                    //    _listWrapper.CollectionChanged -= _listWrapper_CollectionChanged;
                    //}

                    Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
