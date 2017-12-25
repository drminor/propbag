using DRM.TypeSafePropertyBag;
using System;
using System.Windows.Data;

using System.Collections.ObjectModel;

namespace DRM.PropBagWPF
{
    using System.ComponentModel;
    using PropIdType = UInt32;
    using PropNameType = String;

    public class CViewSourceProp<T> : PropTypedBase<CollectionViewSource>, ICViewSourceProp<CollectionViewSource, T> 
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

        object ICViewSourceProp<CollectionViewSource, T>.Source
        {
            get
            {
                return Source;
            }
            set
            {
                if(value is ObservableCollection<T> cvs)
                {
                    Source = cvs;
                }
                else
                {
                    throw new InvalidOperationException($"The source value must be an {nameof(ObservableCollection<T>)}.");
                }
            }
        }

        ICollectionView IReadOnlyCViewSourceProp<CollectionViewSource, T>.View
        {
            get
            {
                return View;
            }
        }

        //public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection() => new ReadOnlyObservableCollection<T>(Source);


        public override object Clone() => throw new NotSupportedException($"This Prop Item of type: {typeof(ICViewSourceProp<CollectionViewSource, T>).Name} does not implement the Clone method.");

        public override void CleanUpTyped()
        {
            if(TypedValue is IDisposable disable)
            {
                disable.Dispose();
            }
        }
    }
}
         