using DRM.TypeSafePropertyBag;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.PropBag.Collections
{
    public class PbListSource<T> : IMyListSource<T> where T: class
    {
        //IPropBag _propBag;
        //string _propertyName;

        public PbListSource(Func<ObservableCollection<T>> observableCollectionGetter)
        {
            UseObservable = true;
            ObservableCollectionGetter = observableCollectionGetter ?? throw new ArgumentNullException(nameof(observableCollectionGetter));
        }

        public PbListSource(Func<IEnumerable<T>> enumerableGetter)
        {
            UseObservable = false;
            IEnumerableGetter = enumerableGetter ?? throw new ArgumentNullException(nameof(enumerableGetter));
        }

        #region Constructor



        //public PbListSource(IPropBag propBag, string propertyName)
        //{
        //    _propBag = propBag ?? throw new ArgumentNullException(nameof(propBag));
        //    _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        //}

        #endregion

        public Func<ObservableCollection<T>> ObservableCollectionGetter { get; }

        public Func<IEnumerable<T>> IEnumerableGetter { get;  }

        public bool UseObservable { get;  }

        public Action<IPropBag, string> PersistList { get; }


        private ObservableCollection<T> _theList;
        public ObservableCollection<T> TheList
        {
            get
            {
                if (_theList == null)
                {
                    if (UseObservable)
                    {
                        _theList = ObservableCollectionGetter();
                    }
                    else
                    {
                        _theList = new ObservableCollection<T>(IEnumerableGetter());
                    }
                }
                return _theList;
            }
        }

        public IList GetList()
        {
            return TheList;
            //IProp<T> prop = _propBag.GetTypedProp<T>(_propertyName);
            //T val = prop.TypedValue;
            //return  val;
        }

        public bool ContainsListCollection => false;
    }
}
