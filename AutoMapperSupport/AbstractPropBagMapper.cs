using AutoMapper;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;

using System.Collections.Generic;
using System.Linq;

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
        private readonly ICloneable _template;
        private readonly IPropBagInternal _pbTemplate;

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
                if(_requiresWrappperTypeEmitServices)
                {
                    _template = null;
                    _pbTemplate = (IPropBagInternal)GetNewDestination(PropModel, _storeAccessCreator, DestinationType, PropFactory, fullClassName: null);
                }
                else
                {
                    _template = (ICloneable)GetNewDestination(PropModel, _storeAccessCreator, DestinationType, PropFactory, fullClassName: null);
                    _pbTemplate = null;
                }
            }
            else
            {
                _template = null;
                _pbTemplate = null;
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
            //if(_template != null)
            //{
            //    IPropBagInternal test = (IPropBagInternal)_template;
            //    SimpleLevel2KeyMan sTest = (SimpleLevel2KeyMan) test.Level2KeyManager;
            //}

            //if (_pbTemplate != null)
            //{
            //    IPropBagInternal test = (IPropBagInternal)_pbTemplate;
            //    SimpleLevel2KeyMan sTest = (SimpleLevel2KeyMan)test.Level2KeyManager;
            //}

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
            TDestination result;

            if (_template != null)
            {
                result = _template.Clone() as TDestination;
            }
            else if(_pbTemplate != null)
            {
                result = GetNewDestination(RunTimeType, _pbTemplate);
            }
            else
            {
                result = GetNewDestination(PropModel, _storeAccessCreator, RunTimeType, PropFactory, fullClassName: null);
            }

            //result = GetNewDestination(PropModel, RunTimeType, PropFactory, fullClassName: null);
            return result;
        }

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

        private TDestination GetNewDestination(Type destinationTypeOrProxy, IPropBagInternal copySource)
        {
            try
            {
                var newViewModel = _vmActivator.GetNewViewModel(destinationTypeOrProxy, copySource);
                return newViewModel as TDestination;
            }
            catch (Exception e2)
            {
                Type targetType = destinationTypeOrProxy ?? typeof(TDestination);
                throw new InvalidOperationException($"Cannot create an instance of {targetType} that takes a copySource parameter.", e2);
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
