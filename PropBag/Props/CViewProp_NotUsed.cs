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
using System.Windows.Data;

namespace DRM.PropBag.Collections
{
    public class CViewProp<CT, T> : PropTypedBase<CT>, ICViewProp<CT, T> where CT: ObservableCollection<T>
    {
        ICPropPrivate<CT, T> _sourceList;

        public CViewProp(ICPropPrivate<CT, T> collectionProp, Func<CT, CT, bool> comparer, GetDefaultValueDelegate<CT> defaultValFunc)
            : base(typeof(ICollectionView), collectionProp.TypeIsSolid, collectionProp.HasStore, comparer, defaultValFunc, PropKindEnum.Collection)
        {
            _sourceList = collectionProp;
            if(_sourceList.ValueIsDefined && _sourceList.ObservableCollection != null)
            {
                //_value = CollectionViewSource.GetDefaultView(_sourceList.ObservableCollection);
            }

        }

        CT _value;
        override public CT TypedValue
        {
            get => (CT) _sourceList.ObservableCollection;

            set => throw new InvalidOperationException("$Prop Items implementing the ICViewProp < CT, T > interface cannot have their value set.");

        }

        public override object TypedValueAsObject => (object)TypedValue;

        public override ValPlusType GetValuePlusType()
        {
            return new ValPlusType(TypedValue, Type);
        }

        override public bool ValueIsDefined => _sourceList.ValueIsDefined;

        override public IListSource ListSource => throw new NotSupportedException($"Prop Items implementing the ICViewProp<CT, T> interface do not provide a ListSource.");

        override public bool SetValueToUndefined() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot have their value set to undefined.");

        public override object Clone() => throw new NotSupportedException("Prop Items implementing the ICViewProp<CT, T> interface cannot be cloned.");

        public override void CleanUpTyped()
        {
            // TODO: Dispose of our items.
        }

        public ReadOnlyObservableCollection<T> ReadOnlyObservableCollection
        {
            get
            {
                return _sourceList.ReadOnlyObservableCollection;
            }
        }

        public bool IsListCollectionView { get; }

        #region ICProp<CT, T> Implementation

        //public ReadOnlyObservableCollection<T> ReadOnlyObservableCollection
        //{
        //    get
        //    {
        //        return ListSourceProvider.GetTheReadOnlyList(null);
        //    }
        //}

        #endregion

        #region ICPropPrivate<CT, T> Implementation

        //public ObservableCollection<T> ObservableCollection
        //{
        //    get
        //    {
        //        return ListSourceProvider.GetTheList(null);
        //    }
        //}

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

        //// Note: component is not used in this implementation: it is included in the interface
        //// because some implementations may need additional input.
        //private IList MakeIListWrapper(object component)
        //{
        //    return ListSourceProvider.GetTheList(null);
        //    //if (component is ICProp<IEnumerable<T>, T> typedComponent)
        //    //{
        //    //    return ListSourceProvider.GetTheList(typedComponent);
        //    //}
        //    //else
        //    //{
        //    //    // This implementation does not use the component parameter.
        //    //    return ListSourceProvider.GetTheList(null);
        //    //}
        //}

        //PbListSourceProvider<IEnumerable<T>, ICProp<IEnumerable<T>, T>, T> _listSourceProvider;
        //private PbListSourceProvider<IEnumerable<T>, ICProp<IEnumerable<T>, T>, T> ListSourceProvider
        //{
        //    get
        //    {
        //        if (_listSourceProvider == null)
        //        {
        //            _listSourceProvider = new PbListSourceProvider<IEnumerable<T>, ICProp<IEnumerable<T>, T>, T>(
        //                observableCollectionGetter: GetValueAsObsColl);
        //        }
        //        return _listSourceProvider;
        //    }
        //}

        //// Note: component is not used in this implementation: it is included in the interface
        //// because some implementations may need additional input.
        //private ObservableCollection<T> GetValueAsObsColl(ICProp<IEnumerable<T>, T> component)
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
        //        ObservableCollection<T> result = new ObservableCollection<T>();

        //        foreach(T item in value)
        //        {
        //            result.Add(item);
        //        }
        //        return result;
        //    }
        //}

        #endregion
    }
}
         