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
    public class CProp<CT,T> : PropTypedBase<T>, ICPropPrivate<CT,T> where CT: IEnumerable<T>
    {
        public CProp(T initalValue,
            GetDefaultValueDelegate<T> getDefaultValFunc,
            bool typeIsSolid,
            bool hasStore,
            Action<T, T> doWhenChanged = null,
            bool doAfterNotify = false,
            Func<T, T, bool> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore, doWhenChanged, doAfterNotify, comparer, getDefaultValFunc)
        {
            if (hasStore)
            {
                _value = initalValue;
                _valueIsDefined = true;
            }
        }

        public CProp(GetDefaultValueDelegate<T> getDefaultValFunc,
            bool typeIsSolid = true,
            bool hasStore = true,
            Action<T, T> doWhenChanged = null,
            bool doAfterNotify = false,
            Func<T, T, bool> comparer = null)
            : base(typeof(T), typeIsSolid, hasStore, doWhenChanged, doAfterNotify, comparer, getDefaultValFunc)
        {
            if (hasStore)
            {
                _valueIsDefined = false;
            }
        }

        T _value;
        override public T TypedValue
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
                ICPropPrivate<IEnumerable<T>, T> thisProp = (ICPropPrivate <IEnumerable<T>, T >) this;
                return ListSourceProvider.GetTheReadOnlyList(thisProp);
            }
        }

        #endregion

        #region ICPropPrivate<CT,T> Implementation

        public ObservableCollection<T> ObservableCollection
        {
            get
            {
                ICPropPrivate<IEnumerable<T>, T> thisProp = (ICPropPrivate<IEnumerable<T>, T>)this;
                return ListSourceProvider.GetTheList(thisProp);
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

        PbListSourceProvider<IEnumerable<T>, ICPropPrivate<IEnumerable<T>, T>, T> _listSourceProvider;
        private PbListSourceProvider<IEnumerable<T>, ICPropPrivate<IEnumerable<T>, T>, T> ListSourceProvider
        {
            get
            {
                if(_listSourceProvider == null)
                {
                    _listSourceProvider = new PbListSourceProvider<IEnumerable<T>, ICPropPrivate<IEnumerable<T>, T>, T>(
                        observableCollectionGetter: GetValueAsObsColl);
                }
                return _listSourceProvider;
            }
        }

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

        private IList MakeIListWrapper(object component)
        {
            return new ObservableCollection<T>();
        }

        private ObservableCollection<T> GetValueAsObsColl(ICPropPrivate<IEnumerable<T>, T> component)
        {
            //return ObservableCollection;
            return new ObservableCollection<T>();
            //T value = TypedValue;

            //ObservableCollection<object> result = new ObservableCollection<object>();

            //foreach (var x in value)
            //{
            //    result.Add(x);
            //}

            //return result;

        }

        #endregion

    }
}
         