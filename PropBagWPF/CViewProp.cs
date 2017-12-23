using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class CViewProp<T> : PropTypedBase<CollectionViewSource>, ICViewPropWPF<CollectionViewSource, T> 
    {
        private PropNameType _propertyName { get; set; }

        public CViewProp(CollectionViewSource initialValue, PropNameType propertyName)
            : base(typeof(CollectionViewSource), true, true, true, RefEqualityComparer<CollectionViewSource>.Default.Equals, null, PropKindEnum.CollectionViewSource)
        {
            _propertyName = propertyName;
            if (initialValue != null)
            {
                _value = initialValue;
                _source = (ObservableCollection<T>)initialValue.Source;
            }
            else
            {
                _source = new ObservableCollection<T>();
                _value = new CollectionViewSource()
                {
                    Source = _source
                };
            }
        }


        CollectionViewSource _value;
        override public CollectionViewSource TypedValue
        {
            get
            {
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

        //ICollectionView IReadOnlyCViewProp<CollectionViewSource, T>.View => ((CollectionViewSource)_value)?.View;

        //public ICollectionView this[string key] => throw new NotImplementedException();


        public override object Clone() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot be cloned.");

        public override void CleanUpTyped()
        {
            // TODO: Dispose of our items.
        }

        #region BASIC IMPLE

        //object ICViewProp<TCVS, T>.Source
        //{
        //    get
        //    {
        //        return Source;
        //    }
        //    set
        //    {
        //        Source = (ObservableCollection<T>)value;
        //    }
        //}

        //ListCollectionView IReadOnlyCViewPropWPF<TCVS, T>.this[string key] => throw new NotImplementedException();

        //ReadOnlyObservableCollection<T> IReadOnlyCViewPropWPF<TCVS, T>.GetReadOnlyObservableCollection() => new ReadOnlyObservableCollection<T>(Source);


        #endregion
    }
}
         