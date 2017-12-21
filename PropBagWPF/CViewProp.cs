using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DRM.PropBagWPF
{
    public class CViewProp<TCVS, T> : PropTypedBase<TCVS>, ICViewPropWPF<TCVS, T> where TCVS : CollectionViewSource
    {
        //private ObservableCollection<T> _theSource { get; set; }

        public CViewProp(CollectionViewSource initialValue)
            : base(typeof(TCVS), true, true, true, RefEqualityComparer<CollectionViewSource>.Default.Equals, null, PropKindEnum.CollectionViewSource)
        {
            if(initialValue != null)
            {
                _value = (TCVS)initialValue;
                _source = (ObservableCollection<T>)initialValue.Source;
            }
            else
            {
                _source = new ObservableCollection<T>();
                _value = (TCVS)new CollectionViewSource()
                {
                    Source = _source
                };
            }
        }


        TCVS _value;
        override public TCVS TypedValue
        {
            get
            {
                if (!ValueIsDefined)
                {
                    if (ReturnDefaultForUndefined)
                    {
                        return this.GetDefaultValFunc("Prop object doesn't know the prop's name.");
                    }
                    throw new InvalidOperationException("The value of this property has not yet been set.");
                }
                return _value;
            }
            set
            {
                // TODO: Update _source??
                _value = value;
                ValueIsDefined = true;
            }
        }

        public override object TypedValueAsObject => (object)TypedValue;

        ObservableCollection<T> _source;
        public ObservableCollection<T> Source
        {
            get => _source;
            set
            {
                if(!ReferenceEquals(_source, value))
                {
                    _source = value;
                    _value.Source = _source;
                }
            }
        }

        public ListCollectionView View => (ListCollectionView) ((CollectionViewSource)_value)?.View;

        public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection() => new ReadOnlyObservableCollection<T>(Source);

        object ICViewProp<TCVS, T>.Source
        {
            get
            {
                return Source;
            }
            set
            {
                Source = (ObservableCollection<T>) value;
            }
        }

        ICollectionView IReadOnlyCViewProp<TCVS, T>.View => ((CollectionViewSource)_value)?.View;

        public ICollectionView this[string key] => throw new NotImplementedException();


        public override object Clone() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot be cloned.");

        public override void CleanUpTyped()
        {
            // TODO: Dispose of our items.
        }

        IReadOnlyObsCollection<T> IReadOnlyCViewProp<TCVS, T>.GetReadOnlyObservableCollection()
        {
            ReadOnlyObservableCollection<T> temp = GetReadOnlyObservableCollection();

            IReadOnlyObsCollection<T> result = (IReadOnlyObsCollection<T>)temp;
            return result;
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
         