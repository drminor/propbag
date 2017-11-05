using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections;

namespace DRM.PropBag.Collections
{
    public class CProp<CT,T> : PropTypedBase<CT>, ICPropPrivate<CT,T> where CT: IEnumerable<T>
    {
        public CProp(CT initalValue,
            GetDefaultValueDelegate<CT> getDefaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<CT, CT, bool> comparer,
            EventHandler<PropertyChangedWithTValsEventArgs<CT>> doWhenChangedX = null,
            //Action<CT, CT> doWhenChanged = null,
            bool doAfterNotify = false)
            : base(typeof(CT), typeIsSolid, hasStore, doWhenChangedX, doAfterNotify, comparer,
                  getDefaultValFunc, PropKindEnum.Collection)
        {
            if (hasStore)
            {
                _value = initalValue;
                _valueIsDefined = true;
                //PropKind = PropKindEnum.Collection;
            }
        }

        public CProp(GetDefaultValueDelegate<CT> getDefaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Func<CT, CT, bool> comparer,
            EventHandler<PropertyChangedWithTValsEventArgs<CT>> doWhenChangedX = null,
            //Action<CT, CT> doWhenChanged = null,
            bool doAfterNotify = false)
            : base(typeof(CT), typeIsSolid, hasStore, doWhenChangedX, doAfterNotify, comparer, getDefaultValFunc, PropKindEnum.Collection)
        {
            if (hasStore)
            {
                _valueIsDefined = false;
            }
            //PropKind = PropKindEnum.Collection;
        }

        CT _value;
        override public CT TypedValue
        {
            get
            {
                if (HasStore)
                {
                    if (!_valueIsDefined)
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
                    _valueIsDefined = true;
                    // TODO: Update our IList
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private bool _valueIsDefined;
        override public bool ValueIsDefined
        {
            get
            {
                return _valueIsDefined;
            }
        }

        override public bool SetValueToUndefined()
        {
            bool oldSetting = this._valueIsDefined;
            _valueIsDefined = false;

            return oldSetting;
        }

        #region ICProp<CT, T> Implementation

        public ReadOnlyObservableCollection<T> ReadOnlyObservableCollection
        {
            get
            {
                //ICPropPrivate<IEnumerable<T>, T> thisProp = (ICPropPrivate <IEnumerable<T>, T >) this;
                return ListSourceProvider.GetTheReadOnlyList(null);
            }
        }

        #endregion

        #region ICPropPrivate<CT,T> Implementation

        public ObservableCollection<T> ObservableCollection
        {
            get
            {
                //ICPropPrivate<IEnumerable<T>, T> thisProp = (ICPropPrivate<IEnumerable<T>, T>)this;
                return ListSourceProvider.GetTheList(null);
            }
        }

        public void SetListSource(IListSource value)
        {
            if (value is PbListSource pbListSource)
            {
                _listSource = pbListSource;
            }
            else
            {
                throw new ArgumentException("The value used in SetListSource must be a PBListSource.");
            }
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
                    //ICPropPrivate<IEnumerable<T>, T> thisProp = (ICPropPrivate<IEnumerable<T>, T>)this;
                    _listSource = new PbListSource(MakeIListWrapper, null);
                }
                return _listSource;
            }
        }

        // Note: component is not used in this implementation: it is included in the interface
        // because some implementations may need additional input.
        private IList MakeIListWrapper(object component)
        {
            return ListSourceProvider.GetTheList(null);
            //if (component is ICPropPrivate<IEnumerable<T>, T> typedComponent)
            //{
            //    return ListSourceProvider.GetTheList(typedComponent);
            //}
            //else
            //{
            //    // This implementation does not use the component parameter.
            //    return ListSourceProvider.GetTheList(null);
            //}
        }

        PbListSourceProvider<IEnumerable<T>, ICPropPrivate<IEnumerable<T>, T>, T> _listSourceProvider;
        private PbListSourceProvider<IEnumerable<T>, ICPropPrivate<IEnumerable<T>, T>, T> ListSourceProvider
        {
            get
            {
                if (_listSourceProvider == null)
                {
                    _listSourceProvider = new PbListSourceProvider<IEnumerable<T>, ICPropPrivate<IEnumerable<T>, T>, T>(
                        observableCollectionGetter: GetValueAsObsColl);
                }
                return _listSourceProvider;
            }
        }

        // Note: component is not used in this implementation: it is included in the interface
        // because some implementations may need additional input.
        private ObservableCollection<T> GetValueAsObsColl(ICPropPrivate<IEnumerable<T>, T> component)
        {
            CT value = TypedValue;
            if(value == null)
            {
                return new ObservableCollection<T>();
            }
            else if(typeof(CT) is ObservableCollection<T>)
            {
                ObservableCollection<T> result = value as ObservableCollection<T>;
                return result;
            }
            else
            {
                ObservableCollection<T> result = new ObservableCollection<T>();

                foreach(T item in value)
                {
                    result.Add(item);
                }
                return result;
            }
        }

        #endregion

    }
}
         