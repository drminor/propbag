using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;

namespace DRM.PropBag.Collections
{
    public class PbListSourceProvider<CT, T> : IListSourceProvider<CT, T> /*where PT : IETypedProp<CT, T>*/ where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public Func<ICProp<CT, T>, ObservableCollection<T>> ObservableCollectionGetter { get; }
        public Func<IReadOnlyCProp<CT, T>, ReadOnlyObservableCollection<T>> ReadOnlyObservableCollectionGetter { get; }

        public Func<ICProp<CT, T>, IEnumerable<T>> IEnumerableGetter { get; }
        public bool UseObservable { get; }
        public bool IsReadOnly { get; }

        #region Constructor
        public PbListSourceProvider(Func<ICProp<CT, T>, ObservableCollection<T>> observableCollectionGetter)
        {
            UseObservable = true;
            IsReadOnly = false;
            ObservableCollectionGetter = observableCollectionGetter ?? throw new ArgumentNullException(nameof(observableCollectionGetter));
        }

        public PbListSourceProvider(Func<IReadOnlyCProp<CT, T>, ReadOnlyObservableCollection<T>> readOnlyObservableCollectionGetter)
        {
            UseObservable = true;
            IsReadOnly = true;
            ReadOnlyObservableCollectionGetter = readOnlyObservableCollectionGetter ?? throw new ArgumentNullException(nameof(readOnlyObservableCollectionGetter));
        }

        public PbListSourceProvider(Func<ICProp<CT, T>, IEnumerable<T>> enumerableGetter, bool isReadOnly)
        {
            UseObservable = false;
            IsReadOnly = isReadOnly;
            IEnumerableGetter = enumerableGetter ?? throw new ArgumentNullException(nameof(enumerableGetter));
        }
        #endregion

        private ObservableCollection<T> _theList;
        private ReadOnlyObservableCollection<T> _theReadOnlyList;

        public ObservableCollection<T> GetTheList(ICProp<CT, T> component)
        {
            if (IsReadOnly)
            {
                //throw new InvalidOperationException("This list is readonly.");
            }

            if (_theList == null)
            {
                //if (UseObservable)
                //{
                //    _theList = ObservableCollectionGetter(component);
                //}
                //else
                //{
                //    _theList = new ObservableCollection<T>(IEnumerableGetter(component));
                //}
                _theList = new ObservableCollection<T>(IEnumerableGetter(component));

            }
            return _theList;
        }

        public ReadOnlyObservableCollection<T> GetTheReadOnlyList(ICProp<CT, T> component)
        {
            if (_theReadOnlyList == null)
            {
                if(!IsReadOnly)
                {
                    // Provide a read-only wrapper around the ObservableCollection.
                    _theReadOnlyList =  new ReadOnlyObservableCollection<T>(GetTheList(component));
                }
                else
                {
                    //if (UseObservable)
                    //{
                    //    _theReadOnlyList = ReadOnlyObservableCollectionGetter(component);
                    //}
                    //else
                    //{
                    //    _theReadOnlyList = (IReadOnlyObsCollection<T>) new ReadOnlyObservableCollection<T>(new ObservableCollection<T>(IEnumerableGetter(component)));
                    //}
                    _theReadOnlyList = new ReadOnlyObservableCollection<T>(new ObservableCollection<T>(IEnumerableGetter(component)));

                }
            }
            return _theReadOnlyList;
        }
    }
}
