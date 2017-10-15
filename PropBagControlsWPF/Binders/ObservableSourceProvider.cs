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

        #endregion

        #region Public Methods

        public ObservableSource CreateObservableSource()
        {
            switch (this.SourceKind)
            {
                case SourceKindEnum.Empty: return new ObservableSource(this.PathElement);
                case SourceKindEnum.TerminalNode: return new ObservableSource(this.PathElement, this.Type);
                case SourceKindEnum.DataContext:
                    {
                        if (this.Data is FrameworkElement fe) return new ObservableSource(fe, this.PathElement);
                        if (this.Data is FrameworkContentElement fce) return new ObservableSource(fce, this.PathElement);
                        throw new InvalidOperationException("ObservableSourceProvider of SourceKind = DataContext is neither FrameworkElement or FrameworkContentElement.");
                    }
                case SourceKindEnum.PropertyObject: return new ObservableSource((INotifyPropertyChanged)this.Data, this.PathElement);
                case SourceKindEnum.CollectionObject: return new ObservableSource((INotifyCollectionChanged)this.Data, this.PathElement);
                case SourceKindEnum.DataSourceProvider: return new ObservableSource((DataSourceProvider)this.Data, this.PathElement);
                default: throw new InvalidOperationException("That Source Kind is not recognized.");
            }
        }

        #endregion

        #region Constructors and their handlers

        #region Terminal Node 
        public ObservableSourceProvider(string pathElement, Type type)
        {
            Data = null;
            Type = type;
            SourceKind = SourceKindEnum.TerminalNode;
            PathElement = pathElement;
        }
        #endregion

        #region From Framework Element and Framework Content Element
        public ObservableSourceProvider(FrameworkElement fe, string pathElement)
        {
            Data = fe;
            Type = null;
            SourceKind = SourceKindEnum.DataContext;
            PathElement = pathElement;
        }

        public ObservableSourceProvider(FrameworkContentElement fce, string pathElement)
        {
            Data = fce;
            Type = null;
            SourceKind = SourceKindEnum.DataContext;
            PathElement = pathElement;
        }

        #endregion

        #region From INotifyPropertyChanged
        public ObservableSourceProvider(INotifyPropertyChanged itRaisesPropChanged, string pathElement)
        {
            Data = itRaisesPropChanged ?? throw new ArgumentNullException($"{nameof(itRaisesPropChanged)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.PropertyObject;
            PathElement = pathElement;
        }

        #endregion

        #region From INotifyCollection Changed
        public ObservableSourceProvider(INotifyCollectionChanged itRaisesCollectionChanged, string pathElement)
        {
            Data = itRaisesCollectionChanged ?? throw new ArgumentNullException($"{nameof(itRaisesCollectionChanged)} was null when constructing Observable Source.");
            Type = null;
            SourceKind = SourceKindEnum.CollectionObject;
            PathElement = pathElement;
        }

        #endregion

        #region From DataSourceProvider
        public ObservableSourceProvider(DataSourceProvider dsp, string pathElement)
        {
            Data = dsp;
            Type = null;
            SourceKind = SourceKindEnum.DataSourceProvider;
            PathElement = pathElement;
        }
        #endregion

        #endregion Constructors and their handlers
    }

}
