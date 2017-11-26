using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class ObservableSource<T> : INotifyPCTyped<T>
    {
        #region Public events and properties

        public event EventHandler<DataSourceChangedEventArgs>  DataSourceChanged = null;
        public event EventHandler<PCTypedEventArgs<T>> PropertyChangedWithTVals;

        public string BinderName { get; private set; }
        public PathConnectorTypeEnum PathConnector => PathConnectorTypeEnum.Dot;

        public string PathElement { get; set; }

        public ExKeyT PropId { get; }

        public SourceKindEnum SourceKind { get; }

        public INotifyParentNodeChanged ParentChangedSource { get; }

        public IPropBag PropChangeGenSource { get; }

        public INotifyPCTyped<T> PropChangedTypedSource { get; }


        internal bool GetHasTypeAndHasData(out bool hasData)
        {

            //if (SourceKind == SourceKindEnum.DataSourceProvider)
            //{
            //    hasData = _wrContainer != null && _wrContainer.IsAlive;
            //}
            //else
            //{
            //    hasData = _wrData != null && _wrData.IsAlive;
            //}

            //return _type != null;

            hasData = false;
            return false;

        }

        public bool IsListeningForNewDC { get; private set; }

        public bool IsReadyOrWatching => Status.IsReadyOrWatching();
        public bool IsDcListeningToProp => Status.IsWatchingProp();
        public bool IsDcListeningToColl => Status.IsWatchingColl();
        public bool IsDcListening => Status.IsWatching();

        public ObservableSourceStatusEnum Status { get; private set; }


        #endregion

        #region Public Methods

        public void Reset(EventHandler<DataSourceChangedEventArgs> subscriber)
        {

            //if (subscriber != null) Unsubscribe(subscriber);

            //if (SourceKind != SourceKindEnum.Empty && SourceKind != SourceKindEnum.TerminalNode && Status.IsWatching())
            //{
            //    object data = Data;
            //    if (data != null)
            //    {
            //        RemoveSubscriptions(data);
            //    }
            //}
        }

        //public void UpdateData()
        //{
        //    if (!(SourceKind == SourceKindEnum.FrameworkElement || SourceKind == SourceKindEnum.FrameworkContentElement || SourceKind == SourceKindEnum.DataSourceProvider || SourceKind == SourceKindEnum.DataGridColumn))
        //    {
        //        throw new InvalidOperationException($"Only ObservableSources with SourceKind =" +
        //            $" {nameof(SourceKindEnum.FrameworkElement)} or " +
        //            $"{nameof(SourceKindEnum.FrameworkContentElement)} " +
        //            $"{nameof(SourceKindEnum.DataSourceProvider)} " + 
        //            $"can have their data updated.");
        //    }

        //    object newData;
        //    Type newType = null;
        //    ObservableSourceStatusEnum newStatus = ObservableSourceStatusEnum.NoType;

        //    if (SourceKind == SourceKindEnum.DataSourceProvider)
        //    {
        //        newData = ((DataSourceProvider)Container).Data;
        //        if (newData != null)
        //        {
        //            newType = newData.GetType();
        //        }

        //        newStatus = Status.SetReady(newData != null);
        //    }
        //    else
        //    {
        //        DependencyObject anchor = (DependencyObject)this.AnchorElement;
        //        if (!GetDcFromFrameworkElement(anchor, out newData, out newType, out newStatus))
        //        {
        //            throw new ApplicationException($"TargetObject in {this.BinderName}.ObservableSource was neither a FrameworkElement or a FrameworkContentElement.");
        //        }
        //    }

        //    UpdateData(newData, newType, newStatus);
        //}

        private bool UpdateData(object newData, Type newType, ObservableSourceStatusEnum newStatus)
        {
            //if (!(SourceKind == SourceKindEnum.FrameworkElement
            //    || SourceKind == SourceKindEnum.FrameworkContentElement
            //    || SourceKind == SourceKindEnum.DataSourceProvider
            //    || SourceKind == SourceKindEnum.DataGridColumn))
            //{
            //    throw new InvalidOperationException($"Only ObservableSources with SourceKind =" +
            //        $" {nameof(SourceKindEnum.FrameworkElement)} or " +
            //        $"{nameof(SourceKindEnum.FrameworkContentElement)} " +
            //        $"{nameof(SourceKindEnum.DataSourceProvider)} " +
            //        $"{nameof(SourceKindEnum.DataGridColumn)} " +
            //        $"can have their data updated.");
            //}

            //bool changed;
            //object oldData = Data;

            //if (oldData != null && newData != null)
            //{
            //    if (object.ReferenceEquals(oldData, newData))
            //    {
            //        System.Diagnostics.Debug.WriteLine("Update ObservableSource found identical data already present.");
            //        changed = false;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("Update ObservableSource found (different) data already present.");
            //        changed = true;
            //    }
            //}
            //else if(!(oldData == null && newData == null) && (oldData == null || newData == null))
            //{
            //    changed = true;
            //}
            //else
            //{
            //    changed = false;
            //}

            //// Remove existing subscriptions if any for the existing Data.
            //if (oldData != null && IsDcListening)
            //{
            //    RemoveSubscriptions(oldData);
            //}

            //Data = newData;
            //Type = newType;
            //Status = newStatus;

            //return changed;

            return false;
        }

        //public bool DoesChildExist(string pathElement)
        //{
        //    //if (SourceKind == SourceKindEnum.Empty || SourceKind == SourceKindEnum.TerminalNode)
        //    //{
        //    //    throw new ApplicationException($"The Observable is empty or is the TerminalNode, calling DoesChildExist is not supported.");
        //    //}

        //    //Type parentType = this.Type;
        //    //if (parentType == null) return false;

        //    //if (IsPropBagBased)
        //    //{
        //    //    return DoesChildExist_PropBag((IPropBag)Data, parentType, pathElement);
        //    //}
        //    //else
        //    //{
        //    //    return DoesChildExist_Clr(Data, parentType, pathElement);
        //    //}
        //    return false;
        //}

        //public bool DoesChildExist_PropBag(IPropBag data, Type type, string pathElement)
        //{
        //    return data.PropertyExists(pathElement);
        //}

        //public bool DoesChildExist_Clr(object data, Type type, string pathElement)
        //{
        //    return type.HasDeclaredProperty(pathElement);
        //}

        public ObservableSource<T> GetChild(string pathElement)
        {
            //if (SourceKind == SourceKindEnum.Empty || SourceKind == SourceKindEnum.TerminalNode)
            //{
            //    throw new ApplicationException($"Cannot build a new ObservableSource from an ObservableSource with SourceKind = {nameof(SourceKindEnum.Empty)} or {nameof(SourceKindEnum.TerminalNode)}.");
            //}

            //Type parentType = this.Type;
            //if (parentType == null) return null;

            //if (IsPropBagBased)
            //{
            //    return GetChildFromPropBag((IPropBag)Data, parentType, pathElement);

            //}
            //else
            //{
            //    return GetChildFromClr(Data, parentType, pathElement);
            //}
            return null;
        }

        //private ObservableSource<T> GetChildFromPropBag(IPropBag data, Type type, string pathElement)
        //{
        //    object newData;
        //    Type newType;

        //    if (data.TryGetPropGen(pathElement, null, out IPropData iPg))
        //    {
        //        newData = iPg?.TypedProp?.TypedValueAsObject;
        //        newType = iPg?.TypedProp?.Type;

        //        if (newData != null)
        //        {
        //            ObservableSource<T> child = CreateChild(newData, newType, pathElement);
        //            return child;
        //        }
        //        else
        //        {
        //            return new ObservableSource<T>(pathElement, newType, BinderName);
        //        }
        //    }
        //    else
        //    {
        //        // Property value could not be retreived.
        //        if (data.TryGetTypeOfProperty(pathElement, out newType))
        //        {
        //            // Create an ObservableSource with SourceKind = TerminalNode.
        //            return new ObservableSource<T>(pathElement, newType, BinderName);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //private ObservableSource<T> GetChildFromClr(object data, Type type, string pathElement)
        //{
        //    if (data != null)
        //    {
        //        object val = GetMemberValue(pathElement, data, type, this.PathElement,
        //            out Type pt);

        //        ObservableSource<T> child = CreateChild(val, pt, pathElement);
        //        return child;
        //    }
        //    else
        //    {
        //        // Using reflection, get the type.
        //        Type pt = GetTypeOfPathElement(pathElement, type, this.PathElement);

        //        // Create an ObservableSource with SourceKind = TerminalNode.
        //        return new ObservableSource<T>(pathElement, pt, BinderName);
        //    }
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
            //if (dc == null) return;

            //bool removedIt = false;
            //if (dc is INotifyPropertyChanged pc)
            //{
            //    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
            //        .RemoveHandler(pc, "PropertyChanged", OnPCEvent);
            //    removedIt = true;
            //}

            //if (dc is INotifyCollectionChanged cc)
            //{
            //    WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
            //        .RemoveHandler(cc, "CollectionChanged", OnCCEvent);
            //    removedIt = true;
            //}

            //bool haveData = this._wrData != null && this._wrData.IsAlive;
            //Status = Status.SetReady(haveData);

            //if (!removedIt)
            //{
            //    System.Diagnostics.Debug.WriteLine("Could not remove subscriptions. Object Object does not implement INotifyPropertyChanged, nor does it implement INotifyCollectionChanged.");
            //}
            switch (this.SourceKind)
            {
                case SourceKindEnum.AbsRoot:
                    {
                        goto case SourceKindEnum.Up;
                    }
                case SourceKindEnum.RootUp:
                    {
                        goto case SourceKindEnum.Up;
                    }
                case SourceKindEnum.RootDown:
                    {
                        goto case SourceKindEnum.Down;
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
            //itRaisesPCTGen.PropertyChangedWithGenVals += PropertyChangedWithGenVals_Handler;
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

        #region Type Support

        private Type GetTypeOfPathElement(string pathElement, Type parentType, string parentsPathElement)
        {
            PropertyInfo pi = parentType.GetDeclaredProperty(pathElement);
            if (pi == null)
            {
                throw new InvalidOperationException($"{pathElement} does not exist on data source: {parentsPathElement}.");
            }

            try
            {
                return pi.PropertyType;
            }
            catch
            {
                throw new InvalidOperationException($"Cannot get the type from {pathElement} on source: {parentsPathElement}.");
            }
        }

        private object GetMemberValue(string pathElement, object parent, Type parentType,
            string parentsPathElement, out Type memberType)
        {
            // Let's see if its a field.
            MemberInfo mi = parentType.FindMember(pathElement);

            if (mi == null)
            {
                throw new InvalidOperationException($"{pathElement} does not exist on data source: {parentsPathElement}.");
            }

            // TODO: Handle cases where property accessor has index paramter(s.)
            switch (mi.MemberType)
            {
                case MemberTypes.Property:
                    {
                        memberType = ((PropertyInfo)mi).PropertyType;
                        return ((PropertyInfo)mi).GetValue(parent);
                    }
                case MemberTypes.Field:
                    {
                        memberType = ((FieldInfo)mi).FieldType;
                        return ((FieldInfo)mi).GetValue(parent);
                    }
                case MemberTypes.Method:
                    {
                        memberType = ((MethodInfo)mi).ReturnType;
                        return ((MethodInfo)mi).Invoke(parent, new object[0]);
                    }
                default:
                    {
                        throw new InvalidOperationException($"Members of type {mi.MemberType} are not suppoted. Occured while accessing {pathElement} on {parentsPathElement}.");
                    }
            }

        }

        #endregion
    }

}
