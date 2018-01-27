using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.Fundamentals.Unused
{
    public class ObsCollection<T> : IObsCollection<T>
    {
        private readonly IList<T> _internalList;
        private readonly object _sync = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObsCollection(IList<T> list)
        {
            _internalList = list;
        }

        public ObsCollection()
        {
            _internalList = new List<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _internalList.Add(item);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            _internalList.Clear();
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public Boolean Contains(T item)
        {
            return _internalList.Contains(item);
        }

        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }

        public Boolean Remove(T item)
        {
            var result = _internalList.Remove(item);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            return result;
        }

        public Int32 Count => _internalList.Count;


        public Boolean IsReadOnly => false;

        public Int32 IndexOf(T item) => _internalList.IndexOf(item);

        public void Insert(Int32 index, T item)
        {
            _internalList.Insert(index, item);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(Int32 index)
        {
            _internalList.RemoveAt(index);
            RaiseCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }

        public T this[Int32 index]
        {
            get { return _internalList[index]; }
            set { _internalList[index] = value; }
        }



        //public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        object IList.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        T IReadOnlyList<T>.this[int index] => throw new NotImplementedException();

        //public int Count => throw new NotImplementedException();

        //public bool IsReadOnly => throw new NotImplementedException();

        public bool IsFixedSize => false;

        public object SyncRoot => _sync;

        public bool IsSynchronized => false;

        //public void Add(T item)
        //{
        //    throw new NotImplementedException();
        //}

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        //public void Clear()
        //{
        //    throw new NotImplementedException();
        //}

        //public bool Contains(T item)
        //{
        //    throw new NotImplementedException();
        //}

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        //public void CopyTo(T[] array, int arrayIndex)
        //{
        //    throw new NotImplementedException();
        //}

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        //public IEnumerator<T> GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        //public int IndexOf(T item)
        //{
        //    throw new NotImplementedException();
        //}

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        //public void Insert(int index, T item)
        //{
        //    throw new NotImplementedException();
        //}

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Move(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }

        //public bool Remove(T item)
        //{
        //    throw new NotImplementedException();
        //}

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        //public void RemoveAt(int index)
        //{
        //    throw new NotImplementedException();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        private void RaiseCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(sender, args);
        }

        private void RaisePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(sender, args);
        }
    }

}
