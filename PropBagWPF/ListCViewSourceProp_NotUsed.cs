using DRM.PropBag;
using DRM.PropBag.Collections;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    public class CViewSourceProp<T> : PropTypedBase<CollectionViewSource>, ICPropPrivate<CollectionViewSource,T> 
    {
        public CViewSourceProp(CollectionViewSource initalValue,
            GetDefaultValueDelegate<CollectionViewSource> defaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<CollectionViewSource, CollectionViewSource, bool> comparer)
            : base(typeof(CollectionViewSource), typeIsSolid, hasStore, true, comparer, defaultValFunc, PropKindEnum.Collection)
        {
            if (hasStore)
            {
                _value = initalValue;
            }
        }

        public CViewSourceProp(GetDefaultValueDelegate<CollectionViewSource> defaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<CollectionViewSource, CollectionViewSource, bool> comparer)
            : base(typeof(CollectionViewSource), typeIsSolid, hasStore, false, comparer, defaultValFunc, PropKindEnum.Collection)
        {
            //if (hasStore)
            //{
            //    _valueIsDefined = false;
            //}
        }

        CollectionViewSource _value;
        override public CollectionViewSource TypedValue
        {
            get
            {
                if (HasStore)
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
                else
                {
                    throw new InvalidOperationException();
                }
            }
            set
            {
                if (HasStore)
                {
                    _value = value;
                    ValueIsDefined = true;
                    // TODO: Update our IList
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public override object TypedValueAsObject => (object)TypedValue;

        public override ValPlusType GetValuePlusType()
        {
            return new ValPlusType(TypedValue, Type);
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override void CleanUpTyped()
        {
            // TODO: Dispose of our items.
        }

        public ICollectionView View
        {
            get
            {
                CollectionViewSource val = TypedValue;
                if (val == null)
                    return null;

                return TypedValue.View;
            }
        }

        Dictionary<string, CollectionViewSource> _views;
        public ICollectionView this[string key]
        {
            get
            {
                if (_views == null) _views = new Dictionary<string, CollectionViewSource>();

                if(_views.TryGetValue(key, out CollectionViewSource theView))
                {
                    return theView.View;
                }
                else
                {
                    CollectionViewSource cvs = new CollectionViewSource
                    {
                        Source = TypedValue
                    };

                    _views.Add(key, cvs);
                    return cvs.View;
                }
            }
        }

        #region ICProp<CollectionViewSource, T> Implementation

        public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection()
        {
            ReadOnlyObservableCollection<T> result = TypedValue?.GetReadOnlyObservableCollection();
            return result;
        }

        #endregion

        #region ICPropPrivate<CollectionViewSource,T> Implementation

        public ObservableCollection<T> ObservableCollection
        {
            get
            {
                ObservableCollection<T> result;
                CollectionViewSource val = TypedValue;

                if (val == null)
                {
                    result = new ObservableCollection<T>();
                }
                else
                {
                    result = val.ObservableCollection;
                }

                return result;
            }
        }

        public void SetListSource(IListSource value)
        {
            CollectionViewSource val = TypedValue;
            if (val == null)
                return;

            val.Source = value;
            _listSource = null;
        }

        public void SetListSource(CollectionViewSource value)
        {
            CollectionViewSource val = TypedValue;
            if (val == null)
                return;

            val.Source = value;
            _listSource = null;
        }

        #endregion

        #region IListSource Support

        PbListSource _listSource;
        override public IListSource ListSource
        {
            get
            {
                if (_listSource == null)
                {
                    _listSource = new PbListSource(MakeIListWrapper, null);
                }
                return _listSource;
            }
        }

        // Note: component is not used in this implementation: it is included in the interface
        // because some implementations may need additional input.
        private IList MakeIListWrapper(object component)
        {
            IList result = ObservableCollection;
            //CollectionViewSource val = TypedValue;
            //if (val == null)
            //{
            //    result = new ObservableCollection<T>();
            //}
            //else
            //{
            //    result = TypedValue.ObservableCollection;
            //}

            return result;
        }

        #endregion
    }
}
         