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
using System.Collections.Specialized;

namespace DRM.PropBag
{
    using PropNameType = String;

    public class CProp_New<CT,T> : PropTypedBase_New<CT>, ICProp<CT,T> where CT : class, IReadOnlyList<T>, IList<T>, IEnumerable<T>, IList, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public CProp_New(PropNameType propertyName, CT initalValue, bool typeIsSolid, IPropTemplate<CT> template)
            : base(propertyName, initalValue, typeIsSolid, template)
        {
        }

        public CProp_New(PropNameType propertyName, bool typeIsSolid, IPropTemplate<CT> template)
            : base(propertyName, typeIsSolid, template)
        {
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        #region ICProp<CT, T> Implementation

        public ReadOnlyObservableCollection<T> GetReadOnlyObservableCollection()
        {
            // TODO: Fix Me.
            //return ListSourceProvider.GetTheReadOnlyList(null);
            return null;
        }

        #endregion

        #region ICPropPrivate<CT,T> Implementation

        public ObservableCollection<T> ObservableCollection
        {
            get
            {
                // TODO: Fix Me.
                //return ListSourceProvider.GetTheList(null);
                return null;
            }
        }

        //public void SetListSource(IListSource value)
        //{
        //    if (value is PbListSource pbListSource)
        //    {
        //        _listSource = pbListSource;
        //    }
        //    else
        //    {
        //        throw new ArgumentException("The value used in SetListSource must be a PBListSource.");
        //    }
        //}

        //public void SetListSource(CT value)
        //{
        //    TypedValue = value;
        //    _listSource = null;
        //}

        #endregion

        #region IListSource Support

        //PbListSource _listSource;
        //override public IListSource ListSource
        //{
        //    get
        //    {
        //        if (_listSource == null)
        //        {
        //            _listSource = new PbListSource(MakeIListWrapper, null);
        //        }
        //        return _listSource;
        //    }
        //}

        public bool ContainsListCollection => false;


        //public IList GetList()
        //{
        //    return _listSource.GetList();
        //}

        // Note: component is not used in this implementation: it is included in the interface
        // because some implementations may need additional input.
        private IList MakeIListWrapper(object component)
        {
            // TODO: Fix Me.
            //return ListSourceProvider.GetTheList(null);

            return null;
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

        //PbListSourceProvider<IEnumerable<T>, IETypedProp<IEnumerable<T>, T>, T> _listSourceProvider;
        //private PbListSourceProvider<IEnumerable<T>, IETypedProp<IEnumerable<T>, T>, T> ListSourceProvider
        //{
        //    get
        //    {
        //        if (_listSourceProvider == null)
        //        {
        //            _listSourceProvider = new PbListSourceProvider<IEnumerable<T>, IETypedProp<IEnumerable<T>, T>, T>(
        //                observableCollectionGetter: GetValueAsObsColl);
        //        }
        //        return _listSourceProvider;
        //    }
        //}

        //// Note: component is not used in this implementation: it is included in the interface
        //// because some implementations may need additional input.
        //private ObservableCollection<T> GetValueAsObsColl(IETypedProp<IEnumerable<T>, T> component)
        //{
        //    CT value = TypedValue;
        //    if(value == null)
        //    {
        //        return new ObservableCollection<T>();
        //    }
        //    else if(typeof(CT) is ObservableCollection<T>)
        //    {
        //        ObservableCollection<T> result = value as ObservableCollection<T>;
        //        return result;
        //    }
        //    else
        //    {
        //        ObservableCollection<T> result = new ObservableCollection<T>(value);

        //        //foreach(T item in value)
        //        //{
        //        //    result.Add(item);
        //        //}
        //        return result;
        //    }
        //}

        #endregion
    }
}
         