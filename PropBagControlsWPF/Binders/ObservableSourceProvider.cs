using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class ObservableSourceProvider
    {
        #region Private properties

        public string PathElement { get; private set; }
        public object Data { get; private set; }
        public bool IsTargetADc { get; private set; }
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
                case SourceKindEnum.Empty: return new ObservableSource(PathElement, BinderName);
                case SourceKindEnum.TerminalNode: return new ObservableSource(PathElement, Type, PathConnectorType, BinderName);
                case SourceKindEnum.FrameworkElement: return new ObservableSource((FrameworkElement)Data, PathElement, IsTargetADc, PathConnectorType, BinderName);
                case SourceKindEnum.FrameworkContentElement: return new ObservableSource((FrameworkContentElement)Data, PathElement, IsTargetADc, PathConnectorType, BinderName);
                case SourceKindEnum.DataGridColumn: return new ObservableSource((DataGridColumn)Data, PathElement, PathConnectorType, BinderName);
                case SourceKindEnum.PCGen: return new ObservableSource((INotifyPCGen)Data, PathElement, PathConnectorType, BinderName);
                case SourceKindEnum.PropertyObject: return new ObservableSource((INotifyPropertyChanged)Data, PathElement, PathConnectorType, BinderName);
                case SourceKindEnum.CollectionObject: return new ObservableSource((INotifyCollectionChanged)Data, PathElement, PathConnectorType, BinderName);
                case SourceKindEnum.DataSourceProvider: return new ObservableSource((DataSourceProvider)Data, PathElement, PathConnectorType, BinderName);
                default: throw new InvalidOperationException("That Source Kind is not recognized.");
            }
        }

        #endregion

        #region GetSource Support

        public static ObservableSourceProvider GetSourceRoot(BindingTarget bindingTarget,
            object source, string pathElement, string binderName)
        {
            if (source != null && GetSourceFromSource(source, pathElement, binderName, out ObservableSourceProvider osp))
            {
                return osp;
            }
            else
            {
                GetDefaultSource(bindingTarget, pathElement, binderName, out osp);
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
            else if (source is INotifyPCGen)
            {
                osp = new ObservableSourceProvider(source as INotifyPCGen, pathElement, PathConnectorTypeEnum.Dot, binderName);
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

        public static bool GetDefaultSource(BindingTarget bindingTarget, string pathElement, 
            string binderName, out ObservableSourceProvider osp)
        {
            bool isTargetADc = bindingTarget.IsDataContext;

            if(bindingTarget.DependencyObject is FrameworkElement fe)
            {
                osp = new ObservableSourceProvider(fe, pathElement, isTargetADc, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else if(bindingTarget.DependencyObject is FrameworkContentElement fce)
            {
                osp = new ObservableSourceProvider(fce, pathElement, isTargetADc, PathConnectorTypeEnum.Dot, binderName);
                return true;
            }
            else if(bindingTarget.DependencyObject is DataGridColumn dgc)
            {
                osp = new ObservableSourceProvider(dgc, pathElement, PathConnectorTypeEnum.Dot, binderName);
                return false;
            }
            else
            {
                osp = null;
                return false;
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
            IsTargetADc = false;
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
        public ObservableSourceProvider(FrameworkElement fe, string pathElement, bool isTargetADc, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = fe;
            Type = null;
            SourceKind = SourceKindEnum.FrameworkElement;
            IsTargetADc = isTargetADc;
            
        }

        public ObservableSourceProvider(FrameworkContentElement fce, string pathElement, bool isTargetADc, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = fce;
            Type = null;
            SourceKind = SourceKindEnum.FrameworkContentElement;
            IsTargetADc = isTargetADc;
        }

        public ObservableSourceProvider(DataGridColumn dgc, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = dgc;
            Type = null;
            SourceKind = SourceKindEnum.DataGridColumn;
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

        #region From INotifyPCGen
        public ObservableSourceProvider(INotifyPCGen itRaisesPCGen, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = itRaisesPCGen ?? throw new ArgumentNullException($"{nameof(itRaisesPCGen)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.PCGen;
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
