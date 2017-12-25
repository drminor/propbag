using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;

using System.Collections.ObjectModel;

namespace DRM.PropBagWPF
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class CViewSourceProp<T> : PropTypedBase<CollectionViewSource>, ICViewPropWPF<CollectionViewSource, T> 
    {
        private PropNameType _propertyName { get; set; }

        public CViewSourceProp(CollectionViewSource initialValue, PropNameType propertyName)
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
                _value = value;
                ValueIsDefined = true;
            }
        }

        public override object TypedValueAsObject => TypedValue;

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

        public ListCollectionView View => (ListCollectionView) _value?.View;

        //public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection() => new ReadOnlyObservableCollection<T>(Source);


        public override object Clone() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot be cloned.");

        public override void CleanUpTyped()
        {
            // TODO: Dispose of our items.
        }
    }
}
         