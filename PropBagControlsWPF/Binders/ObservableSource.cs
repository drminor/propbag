using DRM.PropBag.ControlsWPF.WPFHelpers;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class ObservableSource
    {
        #region Public events and properties

        public event DataSourceChangedEventHandler DataSourceChanged = null; // delegate { };

        public string BinderName { get; private set; }
        public PathConnectorTypeEnum PathConnector { get; private set; }

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

        // For a FrameworkElement kind of ObservableSource:
        // The AnchorElement is the (original) targetObject.
        // The Container is the element being watched, either the targetObject or its parent.
        // The DepPropListener hold the PropertyListener listening for DataContextChanged events on the Container element.
        // and the Data is the DataContext of the AnchorElement.

        // For a DataSouceProvider kind of ObservableSource:
        // The Container is the DSP
        // and the Data is the result of accessing DSP.Data.

        // For a DataGridColumn kind of ObservableSource:
        // The AnchorElement is the DataGridColumn, which is being watched.
        // The DepPropListener hold the PropertyListener listening for DisplayIndexChanged events on the DataGridColumn element.
        // and the Data is the ItemsSource of the DataGridColumn's parent DataGrid.

        private WeakReference _wrAnchorElement;
        private Object AnchorElement
        {
            get
            {
                if (_wrAnchorElement == null || !_wrAnchorElement.IsAlive)
                {
                    return null;
                }
                else
                {
                    return _wrAnchorElement.Target;
                }
            }
            set
            {
                if (value != null)
                {
                    _wrAnchorElement = new WeakReference(value);
                }
                else
                {
                    _wrAnchorElement = null;
                }
            }
        }

        private WeakReference<IDisposable> _wrDepPropListener;
        private IDisposable DepPropListener
        {
            get
            {
                if(_wrDepPropListener != null)
                {
                    if(_wrDepPropListener.TryGetTarget(out IDisposable depPropListener))
                    {
                        return depPropListener;
                    }
                }
                return null;
            }
            set
            {
                IDisposable old = DepPropListener;
                if (old != null) old.Dispose();
                if(value != null)
                {
                    _wrDepPropListener = new WeakReference<IDisposable>(value);
                }
            }
        }

        private WeakReference _wrContainer;
        private object Container
        {
            get
            {
                if(!(SourceKind == SourceKindEnum.FrameworkElement 
                    || SourceKind == SourceKindEnum.FrameworkContentElement
                    || SourceKind == SourceKindEnum.DataSourceProvider
                    || SourceKind == SourceKindEnum.DataGridColumn))
                {
                    throw new InvalidOperationException("Attempting to get the value for Container " +
                        $"is not suppoted when the ObservableSouce is not of SourceKind: " +
                        $"{nameof(SourceKindEnum.FrameworkElement)}, " +
                        $"{nameof(SourceKindEnum.FrameworkContentElement)}, " +
                        $"{nameof(SourceKindEnum.DataSourceProvider)} " +
                        $"or {nameof(SourceKindEnum.DataGridColumn)}.");
                }

                if (_wrContainer == null || !_wrContainer.IsAlive) return null;
                return _wrContainer.Target;
            }
            set
            {
                if (!(SourceKind == SourceKindEnum.FrameworkElement
                    || SourceKind == SourceKindEnum.FrameworkContentElement
                    || SourceKind == SourceKindEnum.DataSourceProvider
                    || SourceKind == SourceKindEnum.DataGridColumn))
                {
                    throw new InvalidOperationException("Attempting to get the value for Container " +
                        $"is not suppoted when the ObservableSouce is not of SourceKind: " +
                        $"{nameof(SourceKindEnum.FrameworkElement)}, " +
                        $"{nameof(SourceKindEnum.FrameworkContentElement)}, " +
                        $"{nameof(SourceKindEnum.DataSourceProvider)} " +
                        $"or {nameof(SourceKindEnum.DataGridColumn)}.");
                }

                if (value == null)
                {
                    _wrContainer = null;
                }
                else
                {
                    if(!DoesDataHaveCorrectType(value, SourceKind, out Type type))
                    {
                        throw new InvalidOperationException($"Values of type {type} cannot be used to set the Container of an ObservableSource.");
                    }
                    _wrContainer = new WeakReference(value);
                }

            }
        }

        private bool DoesDataHaveCorrectType(object data, SourceKindEnum sourceKind, out Type type)
        {
            if (data == null)
            {
                type = null;
                return true;
            }

            switch(sourceKind)
            {
                case SourceKindEnum.DataGridColumn:
                    {
                        return typeof(DependencyPropertyListener).IsAssignableFrom(type = data.GetType());
                    }
                case SourceKindEnum.DataSourceProvider:
                    {
                        return typeof(DataSourceProvider).IsAssignableFrom(type = data.GetType());
                    }
                case SourceKindEnum.FrameworkContentElement:
                    {
                        return typeof(FrameworkContentElement).IsAssignableFrom(type = data.GetType());
                    }
                case SourceKindEnum.FrameworkElement:
                    {
                        return typeof(FrameworkElement).IsAssignableFrom(type = data.GetType());
                    }
                default:
                    {
                        // TODO: Check this.
                        type = data.GetType();
                        return true;
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
                else
                {
                    if (_wrData == null || !_wrData.IsAlive)
                    {
                        return null;
                    }
                    else
                    {
                        return _wrData.Target;
                    }
                }
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

        private bool IsTargetADc { get; set; }

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
            if (!(SourceKind == SourceKindEnum.FrameworkElement
                || SourceKind == SourceKindEnum.FrameworkContentElement
                || SourceKind == SourceKindEnum.DataSourceProvider
                || SourceKind == SourceKindEnum.DataGridColumn))
            {
                throw new InvalidOperationException($"Only ObservableSources with SourceKind =" +
                    $" {nameof(SourceKindEnum.FrameworkElement)} or " +
                    $"{nameof(SourceKindEnum.FrameworkContentElement)} " +
                    $"{nameof(SourceKindEnum.DataSourceProvider)} " +
                    $"{nameof(SourceKindEnum.DataGridColumn)} " +
                    $"can have their data updated.");
            }

            bool changed;
            object oldData = Data;

            if (oldData != null && newData != null)
            {
                if (object.ReferenceEquals(oldData, newData))
                {
                    System.Diagnostics.Debug.WriteLine("Update ObservableSource found identical data already present.");
                    changed = false;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Update ObservableSource found (different) data already present.");
                    changed = true;
                }
            }
            else if(!(oldData == null && newData == null) && (oldData == null || newData == null))
            {
                changed = true;
            }
            else
            {
                changed = false;
            }

            // Remove existing subscriptions if any for the existing Data.
            if (oldData != null && IsDcListening)
            {
                RemoveSubscriptions(oldData);
            }

            Data = newData;
            Type = newType;
            Status = newStatus;

            return changed;
        }

        public bool DoesChildExist(string pathElement)
        {
            if (SourceKind == SourceKindEnum.Empty || SourceKind == SourceKindEnum.TerminalNode)
            {
                throw new ApplicationException($"The Observable is empty or is the TerminalNode, calling DoesChildExist is not supported.");
            }

            Type parentType = this.Type;
            if (parentType == null) return false;

            if (IsPropBagBased)
            {
                return DoesChildExist_PropBag((IPropBag)Data, parentType, pathElement);
            }
            else
            {
                return DoesChildExist_Clr(Data, parentType, pathElement);
            }
        }

        public bool DoesChildExist_PropBag(IPropBag data, Type type, string pathElement)
        {
            return data.PropertyExists(pathElement);
        }

        public bool DoesChildExist_Clr(object data, Type type, string pathElement)
        {
            return type.HasDeclaredProperty(pathElement);
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
                return GetChildFromPropBag((IPropBag)Data, parentType, pathElement);

            }
            else
            {
                return GetChildFromClr(Data, parentType, pathElement);
            }
        }

        private ObservableSourceProvider GetChildFromPropBag(IPropBag data, Type type, string pathElement)
        {
            object newData;
            Type newType;

            if (data.TryGetPropGen(pathElement, null, out IPropGen iPg))
            {
                newData = iPg?.TypedProp?.TypedValueAsObject;
                newType = iPg?.TypedProp?.Type;

                if (newData != null)
                {
                    ObservableSourceProvider child = CreateChild(newData, newType, pathElement);
                    return child;
                }
                else
                {
                    return new ObservableSourceProvider(pathElement, newType, PathConnectorTypeEnum.Dot, BinderName);
                }
            }
            else
            {
                // Property value could not be retreived.
                if (data.TryGetTypeOfProperty(pathElement, out newType))
                {
                    // Create an ObservableSource with SourceKind = TerminalNode.
                    return new ObservableSourceProvider(pathElement, newType, PathConnectorTypeEnum.Dot, BinderName);
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
                return new ObservableSourceProvider(pathElement, pt, PathConnectorTypeEnum.Dot, BinderName);
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
                case SourceKindEnum.FrameworkElement:
                    {
                        AddSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.FrameworkContentElement:
                    {
                        AddSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.DataGridColumn:
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
                case SourceKindEnum.FrameworkElement:
                    {
                        RemoveSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.FrameworkContentElement:
                    {
                        RemoveSubscriptions(Data);
                        break;
                    }
                case SourceKindEnum.DataGridColumn:
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
                return new ObservableSourceProvider(data as INotifyPropertyChanged, pathElement, PathConnectorTypeEnum.Dot, BinderName);
            }

            // Collection Changed
            else if (typeof(INotifyCollectionChanged).IsAssignableFrom(type))
            {
                return new ObservableSourceProvider(data as INotifyCollectionChanged, pathElement, PathConnectorTypeEnum.Dot, BinderName);
            }

            // DataSourceProvider
            else if (typeof(DataSourceProvider).IsAssignableFrom(type))
            {
                return new ObservableSourceProvider(data as DataSourceProvider, pathElement, PathConnectorTypeEnum.Dot, BinderName);
            }

            throw new InvalidOperationException("Cannot create child: it does not implement INotifyPropertyChanged, INotifyCollectionChanged, nor is it, or dervive from, a DataSourceProvider.");
        }

        private bool GetDcFromFrameworkElement(object feOrFce, out object dc, out Type type, out ObservableSourceStatusEnum status)
        {
            bool found = LogicalTree.GetDcFromFrameworkElement(feOrFce, out dc, out type);
            status = Status.SetReady(dc != null);

            return found;
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
                string msg = "Cannot create subscriptions. Object does not implement INotifyPropertyChanged, " +
                    "nor does it implement INotifyCollectionChanged.";
                System.Diagnostics.Debug.WriteLine(msg);
                //throw new ApplicationException(msg);
            }
            Status = workStatus;
        }

        private void RemoveSubscriptions(object dc)
        {
            if (dc == null) return;

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

        private ObservableSource(string pathElement, PathConnectorTypeEnum pathConnector, bool isListening, string binderName)
        {
            PathElement = pathElement;
            PathConnector = pathConnector;
            IsListeningForNewDC = isListening;
            BinderName = binderName;

            _wrAnchorElement = null;
            _wrContainer = null;
            _wrData = null;

            NewPathElement = null;
            Type = null;
            IsTargetADc = false;
        }

        #region Empty 
        public ObservableSource(string pathElement, string binderName)
            : this(pathElement, PathConnectorTypeEnum.Dot, false, binderName)
        {
            SourceKind = SourceKindEnum.Empty;
            Data = null;
            Type = null;
            Status = ObservableSourceStatusEnum.NoType;
        }
        #endregion

        #region Terminal Node 
        public ObservableSource(string pathElement, Type type, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, false, binderName)
        {
            SourceKind = SourceKindEnum.TerminalNode;
            Data = null;
            Type = type;
            Status = ObservableSourceStatusEnum.HasType;

        }
        #endregion

        #region From Framework Element and Framework Content Element
        public ObservableSource(FrameworkElement fe, string pathElement, bool isTargetADc, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, true, binderName)
        {
            SourceKind = SourceKindEnum.FrameworkElement;
            AnchorElement = fe;

            UpdateWatcherAndData_Fe(fe, pathElement, isTargetADc, binderName);
        }

        public ObservableSource(FrameworkContentElement fce, string pathElement, bool isTargetADc, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, true, binderName)
        {
            SourceKind = SourceKindEnum.FrameworkContentElement;
            AnchorElement = fce;

            UpdateWatcherAndData_Fe(fce, pathElement, isTargetADc, binderName);
        }

        private void DataContextChanged_Fe(DependencyPropertyChangedEventArgs e)
        {
            DependencyObject feOrFce = (DependencyObject)this.AnchorElement;

            bool changed = UpdateWatcherAndData_Fe(feOrFce, this.PathElement, this.IsTargetADc, this.BinderName);

            // TODO: Determine if a real change has occured,
            // and then raise only if real.
            OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated, changed);
        }

        //private void DataContextChanged_Fe(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    DependencyObject feOrFce = (DependencyObject)this.AnchorElement;
            
        //    bool changed = UpdateWatcherAndData_Fe(feOrFce, this.PathElement, this.IsTargetADc, this.BinderName);

        //    // TODO: Determine if a real change has occured,
        //    // and then raise only if real.
        //    OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated, changed);
        //}

        private void SubscribeTo_FcOrFce(DependencyObject newDepObj, Action<DependencyPropertyChangedEventArgs> action)
        {
            // TODO: Create a new version of DependencyPropertyListener
            // that takes an existing event source so that we can make these event subscriptions use a Weak Reference.

            if(newDepObj is FrameworkElement fe)
            {
                DependencyProperty dataContextProp = LogicalTree.FeDataContextDpPropProvider.Value;
                DepPropListener = fe.ListenToProperty(dataContextProp, action);
            }
            else if(newDepObj is FrameworkContentElement fce)
            {
                DependencyProperty dataContextProp = LogicalTree.FceDataContextDpPropProvider.Value;
                DepPropListener = fce.ListenToProperty(dataContextProp, action);
            }
            else
            {
                // TODO: Throw an InvalidOperationException.
            }
        }

        private bool UpdateWatcherAndData_Fe(DependencyObject targetObject, string pathElement,
            bool isTargetADc, string binderName)
        {
            if (SourceKind != SourceKindEnum.FrameworkElement && SourceKind != SourceKindEnum.FrameworkContentElement)
            {
                throw new InvalidOperationException($"Cannot call {nameof(UpdateWatcherAndData_Fe)} " +
                    $"if the ObservableSource does not have a SourceKind of {nameof(SourceKindEnum.FrameworkElement)} " +
                    $"or {nameof(SourceKindEnum.FrameworkContentElement)}.");
            }

            string fwElementName = LogicalTree.GetNameFromDepObject(targetObject);
            System.Diagnostics.Debug.WriteLine($"Fetching DataContext to use from: {fwElementName} for pathElement: {pathElement}.");

            this.IsTargetADc = isTargetADc;

            // If this binding sets a DataContext, watch the TargetObject's parent, otherwise watch the TargetObject for DataContext updates

            // TODO: May want to make sure that the value of Container is a DependencyObject.
            DependencyObject curContainer = (DependencyObject)Container;
            DependencyObject newContainer = isTargetADc ? LogicalTreeHelper.GetParent(targetObject) : targetObject;

            if(!object.ReferenceEquals(curContainer, newContainer))
            {
                SubscribeTo_FcOrFce(newContainer, DataContextChanged_Fe);
                Container = newContainer;
            }

            // Now see if we can find a data context.
            DependencyObject foundNode = LogicalTree.GetDataContext(targetObject, out bool foundIt,
                startWithParent: isTargetADc, inspectAncestors: true, stopOnNodeWithBoundDc: true);

            if (!foundIt)
            {
                bool changed = UpdateData(null, null, ObservableSourceStatusEnum.NoType);
                return changed;
            }

            if (!object.ReferenceEquals(targetObject, foundNode))
            {
                string foundFwElementName = LogicalTree.GetNameFromDepObject(foundNode);
                System.Diagnostics.Debug.WriteLine($"Found DataContext to watch using an ancestor: {foundFwElementName} for pathElement: {pathElement}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Found DataContext to watch on the target object for pathElement: {pathElement}.");
            }

            DependencyObject foundNodeWithBoundDc = LogicalTree.GetDataContextWithBoundDc(targetObject,
                out bool foundOneWithBoundDc, startWithParent: isTargetADc);

            if (foundOneWithBoundDc)
            {
                System.Diagnostics.Debug.WriteLine("Some parent has a DataContext that is set via a Binding Markup.");
            }

            if (foundNode is FrameworkElement fe)
            {
                Type newType = fe.DataContext?.GetType();
                ObservableSourceStatusEnum newStatus = Status.SetReady(fe.DataContext != null);

                bool changed = UpdateData(fe.DataContext, newType, newStatus);
                //if(newType.IsIListSource())
                //{
                //    changed = UpdateData(((IListSource)fe.DataContext).GetList(), newType, newStatus);
                //}
                //else
                //{
                //    changed = UpdateData(fe.DataContext, newType, newStatus);
                //}

                return changed;
            }
            else if (foundNode is FrameworkContentElement fce)
            {
                Type newType = fce.DataContext?.GetType();
                ObservableSourceStatusEnum newStatus = Status.SetReady(fce.DataContext != null);
                bool changed = UpdateData(fce.DataContext, newType, newStatus);
                return changed;
            }
            else
            {
                throw new ApplicationException($"Found node in {binderName}.ObservableSourceProvider was neither a FrameworkElement or a FrameworkContentElement.");
            }
        }


        #endregion

        #region From DataGridColumn
        public ObservableSource(DataGridColumn dgc, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, true, binderName)
        {
            AnchorElement = dgc;
            SourceKind = SourceKindEnum.DataGridColumn;

            SubscribeTo_Dg(dgc, DisplayIndexChanged_Dg);

            UpdateData_Dg(dgc, pathElement, binderName);

        }

        private void DisplayIndexChanged_Dg(DependencyPropertyChangedEventArgs e)
        {
            DataGridColumn dgc = (DataGridColumn)this.AnchorElement;

            bool changed = UpdateData_Dg(dgc, this.PathElement, this.BinderName);

            // TODO: Determine if a real change has occured by comparing the value of Status before and afer the call 
            // to DgUpdateData and then raise only if appropriate.

            if (Status == ObservableSourceStatusEnum.Ready)
            {
                OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated, changed);
            }
        }

        private bool UpdateData_Dg(DataGridColumn dgc, string pathElement, string binderName)
        {
            if (SourceKind != SourceKindEnum.DataGridColumn)
            {
                throw new InvalidOperationException($"Cannot call {nameof(UpdateData_Dg)} " +
                    $"if the ObservableSource is not of SourceKind: {nameof(SourceKindEnum.DataGridColumn)}.");
            }

            System.Diagnostics.Debug.WriteLine($"Fetching DataGrid for a DataGridColumn for {pathElement}.");

            DataGrid dataGrid = LogicalTree.GetDataGridOwner(dgc);

            Type newType = dataGrid?.GetType();
            ObservableSourceStatusEnum newStatus = Status.SetReady(dataGrid != null);

            bool changed = UpdateData(dataGrid?.SelectedItems, newType, newStatus);
            return changed;
        }

        private void SubscribeTo_Dg(DataGridColumn dcg, Action<DependencyPropertyChangedEventArgs> action)
        {
            DependencyProperty dispIndex = LogicalTree.DataGridColumn_DisplayIndex_DpPropProvider.Value;
            DepPropListener = dcg.ListenToProperty(dispIndex, action);

            //object anchor = AnchorElement;

            //if (anchor != null && anchor is DataGridColumn currentDgc)
            //{
            //    if (Container != null && Container is IDisposable disp)
            //    {
            //        // Free up the previous DependencyPropertyListener.
            //        disp.Dispose();
            //    }
            //}

            //if(action != null)
            //{
            //    DependencyProperty dispIndex = LogicalTree.DataGridColumn_DisplayIndex_DpPropProvider.Value;
            //    Container = dcg.PropertyChanged(dispIndex, action);
            //}
            //else
            //{
            //    Container = null;
            //}
        }

        #endregion

        #region From INotifyPropertyChanged
        public ObservableSource(INotifyPropertyChanged itRaisesPropChanged, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, false, binderName)
        {
            SourceKind = SourceKindEnum.PropertyObject;
            Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
            Type = itRaisesPropChanged.GetType();

            Status = ObservableSourceStatusEnum.Ready;
        }

        private void OnPCEvent(object source, PropertyChangedEventArgs args)
        {
            OnDataSourceChanged(args.PropertyName);
        }
        #endregion

        #region From INotifyCollection Changed
        public ObservableSource(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, false, binderName)
        {
            SourceKind = SourceKindEnum.CollectionObject;
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
        public ObservableSource(DataSourceProvider dsp, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, true, binderName)
        {
            SourceKind = SourceKindEnum.DataSourceProvider;
            Container = dsp;
            Data = null;
            Type = null;

            WeakEventManager<DataSourceProvider, EventArgs>.AddHandler(dsp, "DataChanged", OnDataSourceProvider_DataChanged);

            Status = ObservableSourceStatusEnum.Undetermined;
        }

        private void OnDataSourceProvider_DataChanged(object source, EventArgs args)
        {
            // TODO: Has something changed.
            bool changed = true;
            OnDataSourceChanged(DataSourceChangeTypeEnum.DataContextUpdated, changed);
        }
        #endregion

        #endregion Constructors and their handlers

        #region Raise Event Helpers

        private void OnDataSourceChanged(DataSourceChangeTypeEnum changeType, bool changed)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)?.Invoke(this, new DataSourceChangedEventArgs(changeType, changed));
        }

        private void OnDataSourceChanged(string propertyName)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)?.Invoke(this, new DataSourceChangedEventArgs(propertyName));
        }

        private void OnDataSourceChanged(CollectionChangeAction action, object element)
        {
            Interlocked.CompareExchange(ref DataSourceChanged, null, null)?.Invoke(this, new DataSourceChangedEventArgs(action, element));
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
