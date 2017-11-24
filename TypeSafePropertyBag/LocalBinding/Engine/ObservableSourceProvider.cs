using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

using System.Windows.Data;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    public class ObservableSourceProvider<T>
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

        public ObservableSource<T> CreateObservableSource()
        {
            switch (this.SourceKind)
            {
                case SourceKindEnum.Down: return new ObservableSource<T>(PathElement, BinderName);
                //case SourceKindEnum.TerminalNode: return new ObservableSource<T>(PathElement, Type, PathConnectorType, BinderName);
                ////case SourceKindEnum.FrameworkElement: return new ObservableSource<T>((FrameworkElement)Data, PathElement, IsTargetADc, PathConnectorType, BinderName);
                ////case SourceKindEnum.FrameworkContentElement: return new ObservableSource<T>((FrameworkContentElement)Data, PathElement, IsTargetADc, PathConnectorType, BinderName);
                ////case SourceKindEnum.DataGridColumn: return new ObservableSource<T>((DataGridColumn)Data, PathElement, PathConnectorType, BinderName);
                //case SourceKindEnum.PropertyObject: return new ObservableSource<T>((INotifyPropertyChanged)Data, PathElement, PathConnectorType, BinderName);
                //case SourceKindEnum.CollectionObject: return new ObservableSource<T>((INotifyCollectionChanged)Data, PathElement, PathConnectorType, BinderName);
                //case SourceKindEnum.DataSourceProvider: return new ObservableSource<T>((DataSourceProvider)Data, PathElement, PathConnectorType, BinderName);
                default: throw new InvalidOperationException("That Source Kind is not recognized.");
            }
        }

        #endregion

        #region GetSource Support

        public static ObservableSourceProvider<T> GetSourceRoot(IPropBagProxy bindingTarget,
            object source, string pathElement, string binderName)
        {
            //if (source != null && GetSourceFromSource(source, pathElement, binderName, out ObservableSource<T>Provider<T> osp))
            //{
            //    return osp;
            //}
            //else
            //{
            //    GetDefaultSource(bindingTarget, pathElement, binderName, out osp);
            //    return osp;
            //}
            return new ObservableSourceProvider<T>("", PathConnectorTypeEnum.Dot, "");
        }

        public static bool GetSourceFromSource(object source, string pathElement, string binderName, out ObservableSource<T> osp)
        {
            //if (source is DataSourceProvider)
            //{
            //    osp = new ObservableSource<T>(source as DataSourceProvider, pathElement, PathConnectorTypeEnum.Dot, binderName);
            //    return true;
            //}
            //else if (source is INotifyPropertyChanged)
            //{
            //    osp = new ObservableSource<T>(source as INotifyPropertyChanged, pathElement, PathConnectorTypeEnum.Dot, binderName);
            //    return true;
            //}
            //else if (source is INotifyCollectionChanged)
            //{
            //    osp = new ObservableSource<T>(source as INotifyCollectionChanged, pathElement, PathConnectorTypeEnum.Dot, binderName);
            //    return true;
            //}
            //else
            //{
            //    osp = null;
            //    return false;
            //}
            osp = null;
            return false;
        }

        public static bool GetDefaultSource(IPropBagProxy bindingTarget, string pathElement, 
            string binderName, out ObservableSource<T> osp)
        {
            //bool isTargetADc = bindingTarget.IsDataContext;

            //if(bindingTarget.DependencyObject is FrameworkElement fe)
            //{
            //    osp = new ObservableSource<T>Provider<T>(fe, pathElement, isTargetADc, PathConnectorTypeEnum.Dot, binderName);
            //    return true;
            //}
            //else if(bindingTarget.DependencyObject is FrameworkContentElement fce)
            //{
            //    osp = new ObservableSource<T>Provider<T>(fce, pathElement, isTargetADc, PathConnectorTypeEnum.Dot, binderName);
            //    return true;
            //}
            //else if(bindingTarget.DependencyObject is DataGridColumn dgc)
            //{
            //    osp = new ObservableSource<T>Provider<T>(dgc, pathElement, PathConnectorTypeEnum.Dot, binderName);
            //    return false;
            //}
            //else
            //{
            //    osp = null;
            //    return false;
            //}
            //osp = new ObservableSource<T>("", typeof(T), PathConnectorTypeEnum.Dot, "");
            osp = null;
            return false;
        }

        #endregion

        #region Constructors
        private ObservableSourceProvider(string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
        {
            Data = null;
            Type = null;
            SourceKind = SourceKindEnum.Down;
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
        //public ObservableSource<T>Provider<T>(FrameworkElement fe, string pathElement, bool isTargetADc, PathConnectorTypeEnum pathConnectorType, string binderName)
        //    : this(pathElement, pathConnectorType, binderName)
        //{
        //    Data = fe;
        //    Type = null;
        //    SourceKind = SourceKindEnum.FrameworkElement;
        //    IsTargetADc = isTargetADc;
            
        //}

        //public ObservableSource<T>Provider<T>(FrameworkContentElement fce, string pathElement, bool isTargetADc, PathConnectorTypeEnum pathConnectorType, string binderName)
        //    : this(pathElement, pathConnectorType, binderName)
        //{
        //    Data = fce;
        //    Type = null;
        //    SourceKind = SourceKindEnum.FrameworkContentElement;
        //    IsTargetADc = isTargetADc;
        //}

        //public ObservableSource<T>Provider<T>(DataGridColumn dgc, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
        //    : this(pathElement, pathConnectorType, binderName)
        //{
        //    Data = dgc;
        //    Type = null;
        //    SourceKind = SourceKindEnum.DataGridColumn;
        //}

        #endregion

        #region From INotifyPropertyChanged
        public ObservableSourceProvider(INotifyPropertyChanged itRaisesPropChanged, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.Down;
        }

        #endregion

        #region From INotifyCollection Changed
        public ObservableSourceProvider(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement, PathConnectorTypeEnum pathConnectorType, string binderName)
            : this(pathElement, pathConnectorType, binderName)
        {
            Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.Down;
        }

        #endregion

        #endregion Constructors
    }



}
