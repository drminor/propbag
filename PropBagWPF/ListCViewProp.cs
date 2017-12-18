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
    public class ListCViewProp<CT,T> : PropTypedBase<CT>, IListCViewProp<CT,T> where CT: IOListCollectionView<T>
    {
        public ListCViewProp(CT initalValue,
            GetDefaultValueDelegate<CT> defaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<CT, CT, bool> comparer)
            : base(typeof(CT), typeIsSolid, hasStore, true, comparer, defaultValFunc, PropKindEnum.Collection)
        {
            if (hasStore)
            {
                _value = initalValue;
            }
        }

        public ListCViewProp(GetDefaultValueDelegate<CT> defaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<CT, CT, bool> comparer)
            : base(typeof(CT), typeIsSolid, hasStore, false, comparer, defaultValFunc, PropKindEnum.Collection)
        {
            //if (hasStore)
            //{
            //    _valueIsDefined = false;
            //}
        }

        CT _value;
        override public CT TypedValue
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

        public ListCollectionView View
        {
            get
            {
                CT val = TypedValue;
                if (val == null)
                    return null;

                return TypedValue as ListCollectionView;
            }
        }

        Dictionary<string, CollectionViewSource> _views;
        public ListCollectionView this[string key]
        {
            get
            {
                if (_views == null) _views = new Dictionary<string, CollectionViewSource>();

                if(_views.TryGetValue(key, out CollectionViewSource theView))
                {
                    return (ListCollectionView) theView.View;
                }
                else
                {
                    CollectionViewSource cvs = new CollectionViewSource
                    {
                        Source = TypedValue.ObservableCollection
                    };

                    _views.Add(key, cvs);
                    return (ListCollectionView)cvs.View;
                }
            }
        }

        #region ICProp<CT, T> Implementation

        public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection()
        {
            ReadOnlyObservableCollection<T> result = TypedValue?.GetReadOnlyObservableCollection();
            return result;
        }

        #endregion

        #region ICPropPrivate<CT,T> Implementation

        public ObservableCollection<T> ObservableCollection
        {
            get
            {
                ObservableCollection<T> result = TypedValue?.ObservableCollection;
                return result;
            }
        }

        public void SetListSource(IListSource value)
        {
            CT val = TypedValue;
            if (val == null)
                return;

            val.SetListSource(value);
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
            return TypedValue?.ObservableCollection;
        }

        #endregion
    }
}
         