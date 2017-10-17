using DRM.PropBag.ControlsWPF.WPFHelpers;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class ObservableSourceProvider
    {
        #region Private properties

        public string PathElement { get; private set; }
        public object Data { get; private set; }
        public Type Type { get; private set; }
        public SourceKindEnum SourceKind { get; private set; }
        public PathConnectorTypeEnum PathConnectorType { get; private set; }
        public string BinderName { get; private set; }

        #endregion

        #region Public Methods

        public ObservableSource CreateObservableSource()
        {
            switch (this.SourceKind)
            {
                case SourceKindEnum.Empty: return new ObservableSource(this.PathElement, this.BinderName);
                case SourceKindEnum.TerminalNode: return new ObservableSource(this.PathElement, this.Type, this.PathConnectorType, this.BinderName);
                case SourceKindEnum.DataContext:
                    {
                        if (this.Data is FrameworkElement fe) return new ObservableSource(fe, this.PathElement, targetIsAsDc: false, pathConnectorType: this.PathConnectorType, binderName: this.BinderName);
                        if (this.Data is FrameworkContentElement fce) return new ObservableSource(fce, this.PathElement, targetIsAsDc: false, pathConnectorType: this.PathConnectorType, binderName: this.BinderName);
                        throw new InvalidOperationException("ObservableSourceProvider of SourceKind = DataContext is neither FrameworkElement or FrameworkContentElement.");
                    }
                case SourceKindEnum.DataContextBinder:
                    {
                        if (this.Data is FrameworkElement fe) return new ObservableSource(fe, this.PathElement, targetIsAsDc: false, pathConnectorType: this.PathConnectorType, binderName: this.BinderName);
                        if (this.Data is FrameworkContentElement fce) return new ObservableSource(fce, this.PathElement, targetIsAsDc: false, pathConnectorType: this.PathConnectorType, binderName: this.BinderName);
                        throw new InvalidOperationException("ObservableSourceProvider of SourceKind = DataContext is neither FrameworkElement or FrameworkContentElement.");
                    }
                case SourceKindEnum.PropertyObject: return new ObservableSource((INotifyPropertyChanged)this.Data, this.PathElement, this.PathConnectorType, this.BinderName);
                case SourceKindEnum.CollectionObject: return new ObservableSource((INotifyCollectionChanged)this.Data, this.PathElement, this.PathConnectorType, this.BinderName);
                case SourceKindEnum.DataSourceProvider: return new ObservableSource((DataSourceProvider)this.Data, this.PathElement, this.PathConnectorType, this.BinderName);
                default: throw new InvalidOperationException("That Source Kind is not recognized.");
            }
        }

        #endregion

        #region GetSource Support

        public static ObservableSourceProvider GetSourceRoot(DependencyObject targetObject, object targetProperty,
            object source, string pathElement, string binderName)
        {
            if (source != null && GetSourceFromSource(source, pathElement, binderName, out ObservableSourceProvider osp))
            {
                return osp;
            }
            else
            {
                GetDefaultSource(targetObject, targetProperty, pathElement, binderName, out osp);
                return osp;
            }
        }

        public static bool GetSourceFromSource(object source, string pathElement, string binderName, out ObservableSourceProvider osp)
        {
            if (source is DataSourceProvider)
            {
                osp = new ObservableSourceProvider(source as DataSourceProvider, pathElement, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else if (source is INotifyPropertyChanged)
            {
                osp = new ObservableSourceProvider(source as INotifyPropertyChanged, pathElement, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else if (source is INotifyCollectionChanged)
            {
                osp = new ObservableSourceProvider(source as INotifyCollectionChanged, pathElement, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else
            {
                osp = null;
                return false;
            }
        }

        public static bool GetDefaultSource(DependencyObject targetObject, object targetProperty, string pathElement, 
            string binderName, out ObservableSourceProvider osp)
        {
            if(!CheckTargetProperty(targetProperty, out bool targetIsADc))
            {
                throw new InvalidOperationException("The target property must be a dependency property, or a property accessor that provides access to a DataContext.");
            }

            if(targetObject is FrameworkElement fe)
            {
                osp = new ObservableSourceProvider(fe, pathElement, targetIsADc, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else if(targetObject is FrameworkContentElement fce)
            {
                osp = new ObservableSourceProvider(fce, pathElement, targetIsADc, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else
            {
                osp = null;
                return false;
            }
        }

        // TODO: Fix this; We are assuming that all target properties that are not a DependencyProperty, must be a CLR property for getting/setting the DataContext.
        private static bool CheckTargetProperty(object targetProperty, out bool targetIsADc)
        {
            if(targetProperty is DependencyObject depObj)
            {
                targetIsADc = false;
                return true;
            }
            else
            {
                targetIsADc = true;
                return true;
            }
        }

        #endregion

        #region Constructors
        private ObservableSourceProvider(string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
        {
            Data = null;
            Type = null;
            SourceKind = SourceKindEnum.Empty;
            PathElement = pathElement;
            PathConnectorType = pathConnectorType;
            BinderName = binderName;
        }


        #region Terminal Node 
        public ObservableSourceProvider(string pathElement, Type type, PathConnectorTypeEnum pathConnectorType, string binderName) 
            : this(pathElement, pathConnectorType, binderName)
        {
            SourceKind = SourceKindEnum.TerminalNode;
            Data = null;
            Type = type;
        }
        #endregion

        #region From Framework Element and Framework Content Element
        public ObservableSourceProvider(FrameworkElement fe, string pathElement, bool targetIsADc, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = fe;
            Type = null;
            SourceKind = targetIsADc ? SourceKindEnum.DataContextBinder : SourceKindEnum.DataContext;
        }

        public ObservableSourceProvider(FrameworkContentElement fce, string pathElement, bool targetIsADc, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = fce;
            Type = null;
            SourceKind = targetIsADc ? SourceKindEnum.DataContextBinder : SourceKindEnum.DataContext;
        }

        #endregion

        #region From INotifyPropertyChanged
        public ObservableSourceProvider(INotifyPropertyChanged itRaisesPropChanged, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.PropertyObject;
        }

        #endregion

        #region From INotifyCollection Changed
        public ObservableSourceProvider(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.CollectionObject;
        }

        #endregion

        #region From DataSourceProvider
        public ObservableSourceProvider(DataSourceProvider dsp, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = dsp;
            Type = null;
            SourceKind = SourceKindEnum.DataSourceProvider;
        }
        #endregion

        #endregion Constructors
    }

}
