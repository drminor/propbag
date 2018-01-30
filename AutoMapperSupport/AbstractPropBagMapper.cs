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

        private readonly bool _requiresWrappperTypeEmitServices;
        private readonly TDestination _template;

        MemConsumptionTracker _mct = new MemConsumptionTracker(enabled: false);

        #endregion

        #region Constructor

        public AbstractPropBagMapper(IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper, IViewModelActivator vmActivator, PSAccessServiceCreatorInterface storeAccessCreator)
        {
            SourceType = mapRequest.SourceTypeDef.TargetType;
            DestinationType = mapRequest.DestinationTypeDef.TargetType;

            RunTimeType = mapRequest.DestinationTypeDef.NewWrapperType ?? DestinationType; 
            PropModel = mapRequest.DestinationTypeDef.PropModel;
            PropFactory = mapRequest.DestinationTypeDef.PropFactory;

            Mapper = mapper;
            _vmActivator = vmActivator;
            _storeAccessCreator = storeAccessCreator;

            SupportsMapFrom = true;

            _requiresWrappperTypeEmitServices = mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices;

            if (typeof(TDestination) is ICloneable)
            {
                _mct.Measure();

                _template = GetNewDestination(PropModel, _storeAccessCreator, DestinationType, PropFactory, fullClassName: null);
                _template = null; // TODO: Fix Me

                _mct.MeasureAndReport("GetNewDestination(PropModel, ... [In Constructor]", "AbstractPropBagMapper");
            }
            else
            {
                _template = null;
            }

            return;

            //object GndForSizer()
            //{
            //    return GetNewDestination(PropModel, _storeAccessCreator, DestinationType, PropFactory, fullClassName: null);
            //}

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

            if (_template != null)
            {
                result = _template.Clone() as TDestination;
                _mct.MeasureAndReport("_template.Clone", "AstractPropBagMapper");
            }
            else
            {
                result = GetNewDestination(PropModel, _storeAccessCreator, RunTimeType, PropFactory, fullClassName: null);
                _mct.MeasureAndReport("GetNewDestination(PropModel, ...", "AstractPropBagMapper");
            }

            return result;

            //object GndForSizer()
            //{
            //    if (_template != null)
            //    {
            //        result = _template.Clone() as TDestination;
            //    }
            //    else if (_pbTemplate != null)
            //    {
            //        result = GetNewDestination(RunTimeType, _pbTemplate);
            //    }
            //    else
            //    {
            //        result = GetNewDestination(PropModel, _storeAccessCreator, RunTimeType, PropFactory, fullClassName: null);
            //    }
            //    return result;
            //}

        }

        // Regular Instantiation using the PropModel. 
        private TDestination GetNewDestination(IPropModel propModel, PSAccessServiceCreatorInterface storeAccessCreator, Type destinationTypeOrProxy, IPropFactory propFactory, string fullClassName)
        {
            try
            {
                var newViewModel = _vmActivator.GetNewViewModel(propModel, storeAccessCreator, destinationTypeOrProxy, propFactory, fullClassName);
                return newViewModel as TDestination;
            }
            catch (Exception e2)
            {
                Type targetType = destinationTypeOrProxy ?? typeof(TDestination);
                throw new InvalidOperationException($"Cannot create an instance of {targetType} that takes a PropModel parameter.", e2);
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
