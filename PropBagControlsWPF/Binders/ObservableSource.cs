using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class ObservableSource
    {
        #region Public events and properties

        public event DataSourceChangedEventHandler DataSourceChanged = null; // delegate { };

        string _pathElement;
        public string PathElement
        {
            get
            {
                return _pathElement;
            }
            private set
            {
                if (value == "PersonCollectionVM")
                {
                    System.Diagnostics.Debug.WriteLine($"Building ObservableSource for {value}");
                }
                _pathElement = value;
            }
        }

        public string NewPathElement { get; set; }

        // For an ObservableSource of SourceKind: DataContext, the Container is the FrameworkElement
        // and the Data is the DataContext.

        // For an ObservableSource of SourceKind: DataSourceProvider, the Container is the DSP
        // and the Data is the result of accessing DSP.Data.

        private bool _fcOrFce;
        private WeakReference _wrContainer;
        private object Container
        {
            get
            {
                //if (_containerWasNull) return null;
                if (_wrContainer == null) return null;
                if (SourceKind == SourceKindEnum.DataContext)
                {
                    if (_wrContainer.IsAlive)
                    {
                        if (_fcOrFce)
                        {
                            FrameworkContentElement fce = (FrameworkContentElement)_wrContainer.Target;
                            return fce;
                        }
                        else
                        {
                            FrameworkElement fe = (FrameworkElement)_wrContainer.Target;
                            return fe;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (SourceKind == SourceKindEnum.DataSourceProvider)
                {
                    if (_wrContainer.IsAlive)
                    {
                        DataSourceProvider dsp = (DataSourceProvider)_wrContainer.Target;
                        return dsp;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // TODO: Fix this up.
                    throw new InvalidOperationException("Attempting to get the value for Container is not valid when the SourceKind is not DataContext or DataSourceProvider.");
                }
            }
            set
            {
                if (SourceKind == SourceKindEnum.DataContext)
                {
                    if (value == null)
                    {
                        //_containerWasNull = true;
                        _wrContainer = null;
                        _fcOrFce = false;
                    }
                    else
                    {
                        //_containerWasNull = false;
                        _wrContainer = new WeakReference(value);
                        if (value is FrameworkElement)
                        {
                            _fcOrFce = false;
                        }
                        else if (value is FrameworkContentElement)
                        {
                            _fcOrFce = true;
                        }
                        else
                        {
                            throw new InvalidOperationException("Only FrameworkElements and FrameworkContentElements can be use to set the value of the container for an ObservableSource with SourceKind = DataContext.");
                        }
                    }
                }
                else if (SourceKind == SourceKindEnum.DataSourceProvider)
                {
                    if (value == null)
                    {
                        //_containerWasNull = true;
                        _wrContainer = null;
                    }
                    else
                    {
                        //_containerWasNull = false;
                        _wrContainer = new WeakReference(value);
                        if (!(value is DataSourceProvider))
                        {
                            throw new InvalidOperationException("Only DataSourceProviders can be used to set the value of the container for an ObservableSource with SourceKind = DataSourceProvider.");
                        }
                    }
                    _fcOrFce = false;
                }
                else
                {
                    // TODO: Fix this up.
                    throw new InvalidOperationException("Attempting to set the value for Container is not valid when the SourceKind is not DataContext or DataSourceProvider.");
                }

            }
        }

        private WeakReference _wrData;
        public object Data
        {
            get
            {
                if (SourceKind == SourceKindEnum.DataSourceProvider)
                {
                    if (_wrData == null)
                    {
                        DataSourceProvider dsp = (DataSourceProvider)Container;
                        if (dsp == null) return null;

                        object data = dsp.Data;
                        if (data != null)
                        {
                            // Save weak reference in case this value is accessed again.
                            _wrData = new WeakReference(data);
                        }
                        return data;
                    }
                    else
                    {
                        if (_wrData.IsAlive)
                        {
                            return _wrData.Target;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else //if(SourceKind != SourceKindEnum.DependencyObject && SourceKind != SourceKindEnum.Empty && SourceKind != SourceKindEnum.TerminalNode)
                {
                    if (_wrData == null || !(_wrData.IsAlive))
                    {
                        return null;
                    }
                    else
                    {
                        return _wrData.Target;
                    }
                }
                //else
                //{
                //    throw new InvalidOperationException("Only PropertyChanged is supported.");
                //}
            }
            private set
            {
                // TODO: Consider checking the type of the value to see if it's valid
                // for this instance's SourceKind.
                if (value == null)
                {
                    _wrData = null;
                }
                else
                {
                    _wrData = new WeakReference(value);
                }
            }
        }

        // TODO: This should not be needed -- we should be able to determine type upon construction.
        private Type GetTypeOfData(object data)
        {
            if (data == null)
            {
                return null;
            }
            else
            {
                return data.GetType();
            }
        }

        private Type _type;
        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    _type = GetTypeOfData(Data);
                }
                return _type;
            }
            private set
            {
                _type = value;
            }
        }

        internal bool GetHasTypeAndHasData(out bool hasData)
        {

            if (SourceKind == SourceKindEnum.DataSourceProvider)
            {
                hasData = _wrContainer != null && _wrContainer.IsAlive;
            }
            else
            {
                hasData = _wrData != null && _wrData.IsAlive;
            }

            return _type != null;

        }

        public SourceKindEnum SourceKind { get; private set; }

        public bool IsListeningForNewDC { get; private set; }

        public bool IsReadyOrWatching => Status.IsReadyOrWatching();
        public bool IsDcListeningToProp => Status.IsWatchingProp();
        public bool IsDcListeningToColl => Status.IsWatchingColl();
        public bool IsDcListening => Status.IsWatching();

        public ObservableSourceStatusEnum Status { get; private set; }

        public bool IsPropBagBased
        {
            get
            {
                Type test = Type;
                return test == null ? false : test.IsPropBagBased();
            }
        }

        //public bool IsPossiblyPropBagBased
        //{
        //    get
        //    {
        //        Type test = Type;
        //        return test == null ? true : test.IsPropBagBased();
        //    }
        //}

        #endregion

        #region Public Methods

        public void Reset(DataSourceChangedEventHandler subscriber = null)
        {
            if (subscriber != null) Unsubscribe(subscriber);

            if (SourceKind != SourceKindEnum.Empty && SourceKind != SourceKindEnum.TerminalNode && Status.IsWatching())
            {
                object data = Data;
                if (data != null)
                {
                    RemoveSubscriptions(data);
                }
            }
        }

        public void UpdateData(string binderInstanceName)
        {
            if (!(SourceKind == SourceKindEnum.DataContext || SourceKind == SourceKindEnum.DataSourceProvider))
            {
                throw new InvalidOperationException($"Only ObservableSources with SourceKind = {nameof(SourceKindEnum.DataContext)} or {nameof(SourceKindEnum.DataSourceProvider)} can have their data updated.");
            }

            object oldData = Data;
            object newData;
            Type newType = null;
            ObservableSourceStatusEnum newStatus = ObservableSourceStatusEnum.NoType;

            if (SourceKind == SourceKindEnum.DataSourceProvider)
            {
                newData = ((DataSourceProvider)Container).Data;
                if (newData != null)
                {
                    newType = newData.GetType();
                }

                newStatus = Status.SetReady(newData != null);
            }
            else
            {
                if (!GetDcFromFrameworkElement(Container, out newData, out newType, out newStatus))
                {
                    throw new ApplicationException($"TargetObject in {binderInstanceName}.ObservableSource was neither a FrameworkElement or a FrameworkContentElement.");
                }
            }

            if (oldData != null && newData != null)
            {
                if (object.ReferenceEquals(oldData, newData))
                {
                    System.Diagnostics.Debug.WriteLine("Update ObservableSource found identical data already present.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Update ObservableSource found (different) data already present.");
                }
            }

            // TODO: consolidate this -- they use the same logic.
            if (this.SourceKind == SourceKindEnum.DataContext)
            {
                // Remove existing subscriptions if any for the existing Data.
                if (oldData != null && IsDcListening)
                {
                    RemoveSubscriptions(oldData);
                }

                Data = newData;
                Type = newType;
                Status = newStatus;
            }
            else
            {
                // Our SourceKind must be DataSourceProvider.

                // Remove event hander from old data if present.
                if (oldData != null && IsDcListening)
                {
                    RemoveSubscriptions(oldData);
                }

                Data = newData;
                Type = newType;
                Status = newStatus;
            }
        }

        public ObservableSourceProvider GetChild(string pathElement)
        {
            if (SourceKind == SourceKindEnum.Empty || SourceKind == SourceKindEnum.TerminalNode)
            {
                throw new ApplicationException($"Cannot build a new ObservableSource from an ObservableSource with SourceKind = {nameof(SourceKindEnum.Empty)} or {nameof(SourceKindEnum.TerminalNode)}.");
            }

            Type parentType = this.Type;
            if (parentType == null) return null;

            if (IsPropBagBased)
            {
                return GetChildFromPropBag((IPropBagMin)Data, parentType, pathElement);
            }
            else
            {
                return GetChildFromClr(Data, parentType, pathElement);
            }
        }

        private ObservableSourceProvider GetChildFromPropBag(IPropBagMin data, Type type, string pathElement)
        {
            object newData;
            Type newType;

            if (data.TryGetPropGen(pathElement, null, out IPropGen iPg))
            {
                if (iPg is PropGen)
                {
                    newData = ((PropGen)iPg).TypedProp?.TypedValueAsObject;
                    newType = ((PropGen)iPg).Type;
                }
                else
                {
                    newData = iPg?.TypedProp?.TypedValueAsObject;
                    newType = iPg?.Type;
                }

                if (newData != null)
                {
                    ObservableSourceProvider child = CreateChild(newData, newType, pathElement);
                    return child;
                }
                else
                {
                    return new ObservableSourceProvider(pathElement, newType);
                }
            }
            else
            {
                // Property value could not be retreived.
                if (data.TryGetTypeOfProperty(pathElement, out newType))
                {
                    // Create an ObservableSource with SourceKind = TerminalNode.
                    return new ObservableSourceProvider(pathElement, newType);
                }
                else
                {
                    return null;
                }
            }

        }

        private ObservableSourceProvider GetChildFromClr(object data, Type type, string pathElement)
        {
            if (data != null)
            {
                object val = GetMemberValue(pathElement, data, type, this.PathElement,
                    out Type pt);

                ObservableSourceProvider child = CreateChild(val, pt, pathElement);
                return child;
            }
            else
            {
                // Using reflection, get the type.
                Type pt = GetTypeOfPathElement(pathElement, type, this.PathElement);

                // Create an ObservableSource with SourceKind = TerminalNode.
                return new ObservableSourceProvider(pathElement, pt);
            }
        }

        public bool Subscribe(DataSourceChangedEventHandler subscriber)
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

        public bool Unsubscribe(DataSourceChangedEventHandler subscriber)
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

        public void BeginListeningToSource()
        {
            switch (SourceKind)
            {
                case SourceKindEnum.PropertyObject:
                    {
                        AddSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.CollectionObject:
                    {
                        AddSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.DataContext:
                    {
                        AddSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.DataSourceProvider:
                    {
                        AddSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        goto case SourceKindEnum.Empty;
                    }
                case SourceKindEnum.Empty:
                    {
                        break;
                    }
                default:
                    {
                        string msg = $"ObservableSouce with SourceKind value: {SourceKind} is not supported or " +
                            "is not recognized on call to BeginListeningToSource.";

                        System.Diagnostics.Debug.WriteLine(msg);
                        break;
                    }
            }
        }

        public void StopListeningToSource()
        {
            switch (SourceKind)
            {
                case SourceKindEnum.PropertyObject:
                    {
                        RemoveSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.CollectionObject:
                    {
                        RemoveSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.DataContext:
                    {
                        RemoveSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.DataSourceProvider:
                    {
                        RemoveSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.TerminalNode:
                    {
                        goto case SourceKindEnum.Empty;
                    }
                case SourceKindEnum.Empty:
                    {
                        break;
                    }
                default:
                    {
                        string msg = $"ObservableSouce with SourceKind value: {SourceKind} is not supported or " +
                            "is not recognized on call to StopListeningToSource.";

                        System.Diagnostics.Debug.WriteLine(msg);
                        break;
                    }
            }
        }

        #endregion

        #region Private Methods

        private ObservableSourceProvider CreateChild(object data, Type type, string pathElement)
        {
            // Property Changed
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
            {
                return new ObservableSourceProvider(data as INotifyPropertyChanged, pathElement);
            }

            // Collection Changed
            else if (typeof(INotifyCollectionChanged).IsAssignableFrom(type))
            {
                return new ObservableSourceProvider(data as INotifyCollectionChanged, pathElement);
            }

            // DataSourceProvider
            else if (typeof(DataSourceProvider).IsAssignableFrom(type))
            {
                return new ObservableSourceProvider(data as DataSourceProvider, pathElement);
            }

            throw new InvalidOperationException("Cannot create child: it does not implement INotifyPropertyChanged, INotifyCollectionChanged, nor is it, or dervive from, a DataSourceProvider.");
        }

        private bool GetDcFromFrameworkElement(object feOrFce, out object dc, out Type type, out ObservableSourceStatusEnum status)
        {
            type = null;
            status = ObservableSourceStatusEnum.NoType;
            if (feOrFce is FrameworkElement fe)
            {
                dc = fe.DataContext;
                if (dc != null)
                {
                    type = dc.GetType();
                }
                status = Status.SetReady(dc != null);
                return true;
            }
            else if (feOrFce is FrameworkContentElement fce)
            {
                dc = fce.DataContext;
                if (dc != null)
                {
                    type = dc.GetType();
                }
                status = Status.SetReady(dc != null);
                return true;
            }
            else
            {
                dc = null;
                return false;
            }
        }

        private void AddSubscriptions(object dc)
        {
            ObservableSourceStatusEnum workStatus = ObservableSourceStatusEnum.Undetermined;

            bool addedIt = false;
            if (dc is INotifyPropertyChanged pc)
            {
                WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                    .AddHandler(pc, "PropertyChanged", OnPCEvent);

                workStatus = ObservableSourceStatusEnum.IsWatchingProp;
                addedIt = true;
            }

            if (dc is INotifyCollectionChanged cc)
            {
                WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                    .AddHandler(cc, "CollectionChanged", OnCCEvent);

                workStatus = workStatus.SetWatchingColl();
                addedIt = true;
            }

            if (!addedIt)
            {
                throw new ApplicationException("Cannot create subscriptions. Object does not implement INotifyPropertyChanged, nor does it implement INotifyCollectionChanged.");
            }
            Status = workStatus;
        }

        private void RemoveSubscriptions(object dc)
        {
            bool removedIt = false;
            if (dc is INotifyPropertyChanged pc)
            {
                WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                    .RemoveHandler(pc, "PropertyChanged", OnPCEvent);
                removedIt = true;
            }

            if (dc is INotifyCollectionChanged cc)
            {
                WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
                    .RemoveHandler(cc, "CollectionChanged", OnCCEvent);
                removedIt = true;
            }

            bool haveData = this._wrData != null && this._wrData.IsAlive;
            Status = Status.SetReady(haveData);

            if (!removedIt)
            {
                System.Diagnostics.Debug.WriteLine("Could not remove subscriptions. Object Object does not implement INotifyPropertyChanged, nor does it implement INotifyCollectionChanged.");
            }
        }

        #endregion

        #region Constructors and their handlers

        private ObservableSource(SourceKindEnum sourceKind, string pathElement, bool isListening)
        {
            this.SourceKind = sourceKind;
            PathElement = pathElement;
            IsListeningForNewDC = isListening;

            _wrData = null;
            _wrContainer = null;

            NewPathElement = null;
            Type = null;
        }

        #region Empty 
        public ObservableSource(string pathElement)
            : this(SourceKindEnum.Empty, pathElement, false)
        {
            Data = null;
            Type = null;
            Status = ObservableSourceStatusEnum.NoType;
        }
        #endregion

        #region Terminal Node 
        public ObservableSource(string pathElement, Type type)
            : this(SourceKindEnum.TerminalNode, pathElement, false)
        {
            Data = null;
            Type = type;
            Status = ObservableSourceStatusEnum.HasType;

        }
        #endregion

        #region From Framework Element and Framework Content Element
        public ObservableSource(FrameworkElement fe, string pathElement)
            : this(SourceKindEnum.DataContext, pathElement, true)
        {
            Container = fe;
            Data = fe.DataContext;

            // TOOD: Verify that these already use weak events.
            fe.DataContextChanged += Fe_or_fce_DataContextChanged;

            if (fe.DataContext != null)
            {
                Type = fe.DataContext.GetType();
            }
            Status = Status.SetReady(fe.DataContext != null);
        }

        public ObservableSource(FrameworkContentElement fce, string pathElement)
            : this(SourceKindEnum.DataContext, pathElement, true)
        {
            Container = fce;
            Data = fce.DataContext;

            // TOOD: Verify that these already use weak events.
            fce.DataContextChanged += Fe_or_fce_DataContextChanged;

            if (fce.DataContext != null)
            {
                Type = fce.DataContext.GetType();
            }
            Status = Status.SetReady(fce.DataContext != null);
        }

        private void Fe_or_fce_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated);
        }
        #endregion

        #region From INotifyPropertyChanged
        public ObservableSource(INotifyPropertyChanged itRaisesPropChanged, string pathElement)
            : this(SourceKindEnum.PropertyObject, pathElement, false)
        {
            Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
            Type = itRaisesPropChanged.GetType();

            Status = ObservableSourceStatusEnum.Ready;

            //WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
            //    .AddHandler(itRaisesPropChanged, "PropertyChanged", OnPCEvent);
        }

        private void OnPCEvent(object source, PropertyChangedEventArgs args)
        {
            OnDataSourceChanged(args.PropertyName);
        }
        #endregion

        #region From INotifyCollection Changed
        public ObservableSource(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement)
            : this(SourceKindEnum.CollectionObject, pathElement, false)
        {
            Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
            Type = itRaisesCollectionChanged.GetType();

            Status = ObservableSourceStatusEnum.Ready;

            //WeakEventManager<INotifyCollectionChanged, CollectionChangeEventArgs>
            //    .AddHandler(itRaisesCollectionChanged, "CollectionChanged", OnCCEvent);
        }

        private void OnCCEvent(object source, CollectionChangeEventArgs args)
        {
            OnDataSourceChanged(args.Action, args.Element);
        }
        #endregion

        #region From DataSourceProvider
        public ObservableSource(DataSourceProvider dsp, string pathElement)
            : this(SourceKindEnum.DataSourceProvider, pathElement, true)
        {
            Container = dsp;
            Data = null;
            Type = null;

            WeakEventManager<DataSourceProvider, EventArgs>
                .AddHandler(dsp, "DataChanged", OnPlainEvent);

            Status = ObservableSourceStatusEnum.Undetermined;
        }

        private void OnPlainEvent(object source, EventArgs args)
        {
            OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated);
        }
        #endregion

        #endregion Constructors and their handlers

        #region Raise Event Helpers

        private void OnDataSourceChanged(DataSourceChangeTypeEnum changeType)
        {
            DataSourceChangedEventHandler handler = Interlocked.CompareExchange(ref DataSourceChanged, null, null);

            if (handler != null)
            {
                handler(this, new DataSourceChangedEventArgs(changeType));
            }
        }

        private void OnDataSourceChanged(string propertyName)
        {
            DataSourceChangedEventHandler handler = Interlocked.CompareExchange(ref DataSourceChanged, null, null);

            if (handler != null)
            {
                handler(this, new DataSourceChangedEventArgs(propertyName));
            }
        }

        private void OnDataSourceChanged(CollectionChangeAction action, object element)
        {
            DataSourceChangedEventHandler handler = Interlocked.CompareExchange(ref DataSourceChanged, null, null);

            if (handler != null)
            {
                handler(this, new DataSourceChangedEventArgs(action, element));
            }
        }

        #endregion Raise Event Helpers

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
