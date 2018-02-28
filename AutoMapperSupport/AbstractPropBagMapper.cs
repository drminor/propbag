using AutoMapper;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.ViewModelTools;
using System;

using System.Collections.Generic;
using System.Linq;
using ObjectSizeDiagnostics;

namespace DRM.PropBag.AutoMapperSupport
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    public abstract class AbstractPropBagMapper<TSource, TDestination> 
        : IPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        #region Public and Private Members

        public Type SourceType { get; }
        public Type DestinationType { get; }

        public IPropModel PropModel { get; }
        public Type RunTimeType { get; }

        IPropFactory PropFactory { get; }
        public IMapper Mapper { get; }

        //public Func<TDestination, TSource> RegularInstanceCreator { get; }
        public bool SupportsMapFrom { get; }

        IViewModelActivator _vmActivator;
        PSAccessServiceCreatorInterface _storeAccessCreator;
        IProvideAutoMappers _autoMapperService;

        private readonly bool _requiresWrappperTypeEmitServices;
        private readonly TDestination _destPropBagTemplate;

        MemConsumptionTracker _mct = new MemConsumptionTracker(enabled: false);

        #endregion

        #region Constructor

        public AbstractPropBagMapper
            (
            IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper,
            IViewModelActivator vmActivator,
            PSAccessServiceCreatorInterface storeAccessCreator,
            IProvideAutoMappers autoMapperService
            )
        {
            SourceType = mapRequest.SourceTypeDef.TargetType;
            DestinationType = mapRequest.DestinationTypeDef.TargetType;

            RunTimeType = mapRequest.DestinationTypeDef.NewWrapperType ?? DestinationType; 
            PropModel = mapRequest.DestinationTypeDef.PropModel;
            PropFactory = mapRequest.DestinationTypeDef.PropFactory;

            Mapper = mapper;
            _vmActivator = vmActivator;
            _storeAccessCreator = storeAccessCreator;
            _autoMapperService = autoMapperService;

            SupportsMapFrom = true;

            _requiresWrappperTypeEmitServices = mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices;

            _mct.Measure();

            _destPropBagTemplate = GetDestinationTemplate(RunTimeType);

            _mct.MeasureAndReport("GetNewDestination(PropModel, ... [In Constructor]", "AbstractPropBagMapper");

            return;
        }

        private TDestination GetDestinationTemplate(Type targetType)
        {
            //Type tType;

            //if (_requiresWrappperTypeEmitServices)
            //{
            //    tType = RunTimeType;
            //}
            //else
            //{
            //    tType = DestinationType;
            //}
            //_destPropBagTemplate = GetNewDestination(PropModel, _storeAccessCreator, tType, _autoMapperService, PropFactory, fullClassName: null);

            TDestination result = GetNewDestination(DestinationType, PropModel, _storeAccessCreator, _autoMapperService, PropFactory, fullClassName: null);

            if (TestCreateDest(targetType, result))
            {
                // Fix the template's PropNodeCollection to improve performance.
                _storeAccessCreator.FixPropItemSet(result);
            }
            else
            {
                // It turns out that we cannot use the template after all.
                result = null;
            }

            return result;
        }

        private bool TestCreateDest(Type targetType, TDestination template)
        {
            try
            {
                GetNewDestination(targetType, template);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        #endregion

        #region Type Mapper Function

        public TDestination MapToDestination(TSource s)
        {
            TDestination result = MapToDestination(s, GetNewDestination());
            return result;
        }

        public TDestination MapToDestination(TSource s, TDestination d)
        {
            return (TDestination) Mapper.Map(s, d, SourceType, RunTimeType);
        }

        public TSource MapToSource(TDestination d)
        {
            return (TSource)Mapper.Map(d, RunTimeType, SourceType);
        }

        public TSource MapToSource(TDestination d, TSource s)
        {
            return (TSource) Mapper.Map(d, s, RunTimeType, SourceType);
        }

        #endregion

        #region Mapper Functions for Lists

        public IEnumerable<TDestination> MapToDestination(IEnumerable<TSource> listOfSources)
        {
            return listOfSources.Select(s => MapToDestination(s));
        }

        public IEnumerable<TSource> MapToSource(IEnumerable<TDestination> listOfDestinations)
        {
            return listOfDestinations.Select(d => MapToSource(d));
        }

        #endregion

        #region Create Instance of TDestination

        public TDestination GetNewDestination()
        {
            _mct.Measure();

            TDestination result;

            if (_destPropBagTemplate != null)
            {
                if (_requiresWrappperTypeEmitServices)
                {
                    result = GetNewDestination(RunTimeType, _destPropBagTemplate);
                    _mct.MeasureAndReport("GetNewDestination using the copy constructor", "AstractPropBagMapper");
                }
                else
                {
                    result = (TDestination)_destPropBagTemplate.Clone();
                    _mct.MeasureAndReport("_template.Clone", "AstractPropBagMapper");
                }

                //if (_requiresWrappperTypeEmitServices)
                //{
                //    object xx = _template.Clone();

                //    result = (TDestination)_template.Clone();
                //    _mct.MeasureAndReport("_template.Clone of Emitted Type", "AstractPropBagMapper");
                //}
                //else
                //{
                //    result = (TDestination)_template.Clone();
                //    _mct.MeasureAndReport("_template.Clone", "AstractPropBagMapper");
                //}

                //result = (TDestination)_template.Clone();
                //_mct.MeasureAndReport("_template.Clone", "AstractPropBagMapper");

            }
            else
            {
                result = GetNewDestination(RunTimeType, PropModel, _storeAccessCreator, _autoMapperService, PropFactory, fullClassName: null);
                _mct.MeasureAndReport("GetNewDestination(PropModel, ...", "AstractPropBagMapper");
            }

            return result;
        }

        // Regular Instantiation using the PropModel. 
        private TDestination GetNewDestination(Type destinationOrProxyType, IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator,  IProvideAutoMappers autoMapperService, IPropFactory propFactory, string fullClassName)
        {
            try
            {
                var newViewModel = _vmActivator.GetNewViewModel(destinationOrProxyType, propModel, storeAccessCreator, autoMapperService, propFactory, fullClassName);
                return newViewModel as TDestination;
            }
            catch (Exception e2)
            {
                //Type targetType = destinationOrProxyType ?? typeof(TDestination);
                throw new InvalidOperationException($"Cannot create an instance of {destinationOrProxyType} that takes a PropModel argument.", e2);
            }
        }

        // Emitted Types do not implement clone for the Emitted Type (Clone calls the base type.)
        // So this uses the emitted type's constructor that takes a source IPropBag.
        private TDestination GetNewDestination(Type destinationOrProxyType, IPropBag copySource)
        {
            try
            {
                var newViewModel = _vmActivator.GetNewViewModel(destinationOrProxyType, copySource);
                return newViewModel as TDestination;
            }
            catch (Exception e2)
            {
                throw new InvalidOperationException($"Cannot create an instance of {destinationOrProxyType} that takes a single IPropag argument.", e2);
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (PropFactory != null && PropFactory is IDisposable disable)
                        disable.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
