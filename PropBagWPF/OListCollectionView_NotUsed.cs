using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    public class OListCollectionView<T> : ListCollectionView, IOListCollectionView<T>
    {
        ObservableCollection<T> _ourList;

        public OListCollectionView(IList list) : base(list)
        {
            if(list is ObservableCollection<T> oList)
            {
                _ourList = oList;
            }
            else
            {
                throw new InvalidOperationException("Only ObservableCollections<T> may be used to instantiate OListCollectionViews.");
            }
        }

        public ObservableCollection<T> ObservableCollection => _ourList;

        public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection()
        {
            return new ReadOnlyObservableCollection<T>(_ourList);
        }

        public void SetListSource(IListSource value)
        {
            IList list = value.GetList();

            if(list is ObservableCollection<T> oList)
            {
                _ourList = oList;
            }
            else
            {
                throw new InvalidOperationException($"Only ObservableCollections<T> may be used when calling {nameof(SetListSource)} on OListCollectionViews.");
            }
        }

        public void SetListSource(IOListCollectionView<T> source)
        {
            SetListSource((ICView<T>)source);
        }

        public void SetListSource(ICView<T> source)
        {
            _ourList = source.ObservableCollection;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _ourList.GetEnumerator();
        }
    }
}
