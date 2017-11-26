using System;
using System.Linq;
using System.Threading;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    //using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class ObservableSource<T> : INotifyPCTyped<T>
    {
        #region Public events and properties

        public event EventHandler<DataSourceChangedEventArgs>  DataSourceChanged = null;
        public event EventHandler<PCTypedEventArgs<T>> PropertyChangedWithTVals;

        public string BinderName { get; private set; }
        public PathConnectorTypeEnum PathConnector => PathConnectorTypeEnum.Dot;

        public string PathElement { get; set; }

        //public ExKeyT PropId { get; }

        public SourceKindEnum SourceKind { get; }

        public INotifyParentNodeChanged ParentChangedSource { get; }

        public IPropBag PropChangeGenSource { get; }

        public INotifyPCTyped<T> PropChangedTypedSource { get; }

        //internal bool GetHasTypeAndHasData(out bool hasData)
        //{

        //    //if (SourceKind == SourceKindEnum.DataSourceProvider)
        //    //{
        //    //    hasData = _wrContainer != null && _wrContainer.IsAlive;
        //    //}
        //    //else
        //    //{
        //    //    hasData = _wrData != null && _wrData.IsAlive;
        //    //}

        //    //return _type != null;

        //    hasData = false;
        //    return false;
        //}

        public bool IsListeningForNewDC { get; private set; }

        public bool IsReadyOrWatching => Status.IsReadyOrWatching();
        public bool IsDcListeningToProp => Status.IsWatchingProp();
        public bool IsDcListeningToColl => Status.IsWatchingColl();
        public bool IsDcListening => Status.IsWatching();

        public ObservableSourceStatusEnum Status { get; private set; }

        #endregion

        #region Public Methods

        //public void Reset(EventHandler<DataSourceChangedEventArgs> subscriber)
        //{

        //    //if (subscriber != null) Unsubscribe(subscriber);

        //    //if (SourceKind != SourceKindEnum.Empty && SourceKind != SourceKindEnum.TerminalNode && Status.IsWatching())
        //    //{
        //    //    object data = Data;
        //    //    if (data != null)
        //    //    {
        //    //        RemoveSubscriptions(data);
        //    //    }
        //    //}
        //}

        public bool Subscribe(EventHandler<DataSourceChangedEventArgs> subscriber)
        {
            if (DataSourceChanged == null)
            {
                DataSourceChanged = subscriber;
                return true; // We added it.
            }
            else
            {
                Delegate[] subscriberList = DataSourceChanged.GetInvocationList();
                if (subscriberList.FirstOrDefault((x) => x == (Delegate)subscriber) == null)
                {
                    DataSourceChanged += subscriber;
                    return true; // We added it.
                }
                else
                {
                    return false; // Already there.
                }
            }
        }

        public bool Unsubscribe(EventHandler<DataSourceChangedEventArgs> subscriber)
        {
            if (DataSourceChanged == null)
            {
                return false; // It's not there.
            }
            else
            {
                Delegate[] subscriberList = DataSourceChanged.GetInvocationList();
                if (subscriberList.FirstOrDefault((x) => x == (Delegate)subscriber) == null)
                {
                    return false; // Not there.
                }
                else
                {
                    DataSourceChanged -= subscriber;
                    return true; // We removed it.
                }
            }
        }

        public bool Unsubscribe(EventHandler<PCTypedEventArgs<T>> subscriber)
        {
            if (PropertyChangedWithTVals == null)
            {
                return false; // It's not there.
            }
            else
            {
                Delegate[] subscriberList = PropertyChangedWithTVals.GetInvocationList();
                if (subscriberList.FirstOrDefault((x) => x == (Delegate)subscriber) == null)
                {
                    return false; // Not there.
                }
                else
                {
                    PropertyChangedWithTVals -= subscriber;
                    return true; // We removed it.
                }
            }
        }

        #endregion

        #region Private Methods

        public void RemoveSubscriptions()
        {
            switch (this.SourceKind)
            {
                case SourceKindEnum.AbsRoot:
                    {
                        goto case SourceKindEnum.Up;
                    }
                case SourceKindEnum.Up:
                    {
                        ParentChangedSource.ParentNodeHasChanged -= ParentNodeHasChanged_Handler;
                        break;
                    }
                case SourceKindEnum.Down:
                    {
                        //PropChangeGenSource.PropertyChangedWithGenVals -= PropertyChangedWithGenVals_Handler;
                        PropChangeGenSource.UnSubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));
                        
                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        //PropChangeGenSource.PropertyChangedWithGenVals -= PropertyChangedWithGenVals_Handler;

                        //this.PropChangedTypedSource.PropertyChangedWithTVals -= PropertyChangedWithTVals_Handler;

                        PropChangeGenSource.UnSubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"{this.SourceKind} is not recognized or is not supported.");
                    }
            }
        }

        #endregion

        #region Constructors and their handlers

        #region From INotifyPCTyped<T>

        public ObservableSource(INotifyPCTyped<T> itRaisesPCTyped, string pathElement, 
            SourceKindEnum sourceKind, string binderName)
        {
            PathElement = pathElement;
            IsListeningForNewDC = true;
            BinderName = binderName;

            SourceKind = sourceKind;
            PropChangedTypedSource = itRaisesPCTyped;
            itRaisesPCTyped.PropertyChangedWithTVals += PropertyChangedWithTVals_Handler;
            Status = ObservableSourceStatusEnum.Ready;
        }

        private void PropertyChangedWithTVals_Handler(object sender, PCTypedEventArgs<T> e)
        {
            OnPropertyChangedWithTVals(e);
        }

        #endregion

        #region From INotifyPCGen 

        public ObservableSource(IPropBag propBag, string pathElement,
            SourceKindEnum sourceKind, string binderName)
        {
            PathElement = pathElement;
            IsListeningForNewDC = true;
            BinderName = binderName;

            SourceKind = sourceKind;
            PropChangeGenSource = propBag;
            propBag.SubscribeToPropChanged(PropertyChangedWithGenVals_Handler, PathElement, typeof(T));

            Status = ObservableSourceStatusEnum.Ready;
        }

        private void PropertyChangedWithGenVals_Handler(object sender, PCGenEventArgs e)
        {
            if (e.PropertyName == PathElement)
            {
                OnChildPropertyHasChanged(e);
            }
        }

        #endregion

        #region PropStoreParent

        public ObservableSource(INotifyParentNodeChanged notifyParentChangedSource, string pathElement,
            SourceKindEnum sourceKind, string binderName)
        {
            PathElement = pathElement;
            IsListeningForNewDC = true;
            BinderName = binderName;

            SourceKind = sourceKind;
            ParentChangedSource = notifyParentChangedSource;
            notifyParentChangedSource.ParentNodeHasChanged += ParentNodeHasChanged_Handler;
            Status = ObservableSourceStatusEnum.Ready;
        }

        private void ParentNodeHasChanged_Handler(object sender, PSNodeParentChangedEventArgs e)
        {
            OnParentHasChanged(e);
        }

        #endregion

        #endregion Constructors and their handlers

        #region Raise Event Helpers

        public void OnPropertyChangedWithTVals(PCTypedEventArgs<T> eArgs)
        {
            Interlocked.CompareExchange(ref PropertyChangedWithTVals, null, null)?.Invoke(this, eArgs);
        }

        private void OnChildPropertyHasChanged(PCGenEventArgs eArgs)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)
                ?.Invoke(this, DataSourceChangedEventArgs.NewFromPCGen(eArgs));
        }

        private void OnParentHasChanged(PSNodeParentChangedEventArgs eArgs)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)
                ?.Invoke(this, DataSourceChangedEventArgs.NewFromPSNodeParentChanged(eArgs, PathElement, typeof(T)));
        }

        #endregion
    }
}
