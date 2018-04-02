using AutoMapper;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.TypeExtensions;
using ObjectSizeDiagnostics;
using Swhp.AutoMapperSupport;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swhp.Tspb.PropBagAutoMapperService
{
    using PropItemSetKeyType = PropItemSetKey<String>;
    using PropModelType = IPropModel<String>;
    using ViewModelActivatorInterface = IViewModelActivator<UInt32, String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public abstract class AbstractPropBagMapper<TSource, TDestination> : IPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        #region Public and Private Members

        public Type SourceType { get; }
        public Type DestinationType { get; }

        public PropModelType PropModel { get; }
        //public Type RunTimeType { get; }

        IPropFactory PropFactory { get; }
        public IMapper Mapper { get; }

        //public Func<TDestination, TSource> RegularInstanceCreator { get; }
        public bool SupportsMapFrom { get; }

        public Type TargetRunTimeType => DestinationType;  //=> RunTimeType;

        private readonly ViewModelFactoryInterface _viewModelFactory;
        private readonly IPropBagMapperService _propBagMapperService;

        private readonly IConfigureAMapper<TSource, TDestination> _mappingConfiguration;

        private readonly bool _requiresWrappperTypeEmitServices;
        private readonly TDestination _destPropBagTemplate;

        MemConsumptionTracker _mct = new MemConsumptionTracker(enabled: false);

        #endregion

        #region Constructor

        public AbstractPropBagMapper
            (
            PropModelType propModel,
            IMapper mapper,
            ViewModelFactoryInterface viewModelFactory,
            IPropBagMapperService propBagMapperService,
            //IPropBagMapperRequestKey<TSource, TDestination> mapRequest
            IConfigureAMapper<TSource, TDestination> mappingConfiguration
            )
        {
            //SourceType = mapRequest.SourceType;
            //DestinationType = mapRequest.DestinationType;

            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

            //RunTimeType = mapRequest.DestinationTypeDef.RunTimeType;

            PropModel = propModel; //(PropModelType) mapRequest.DestinationTypeDef.UniqueRef;

            PropFactory = PropModel.PropFactory; // (IPropFactory)mapRequest.DestinationTypeDef.PropFactory;

            Mapper = mapper;

            _viewModelFactory = viewModelFactory;
            _propBagMapperService = propBagMapperService;
            _mappingConfiguration = mappingConfiguration;

            SupportsMapFrom = true;

            //_requiresWrappperTypeEmitServices = mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices;
            //_requiresWrappperTypeEmitServices = DestinationType.CustomAttributes.OfType<WasEmittedAttribute>()

            _requiresWrappperTypeEmitServices = _mappingConfiguration.RequiresWrappperTypeEmitServices;

            CheckRequiresEmitted(_requiresWrappperTypeEmitServices, DestinationType);

            _mct.Measure();

            _destPropBagTemplate = GetDestinationTemplate(DestinationType);

            _mct.MeasureAndReport("GetNewDestination(PropModel, ... [In Constructor]", "AbstractPropBagMapper");

            // Working on see if we can get the SupportsMapFrom from the Mapper itself.
            //TypePair tp = new TypePair(SourceType, DestinationType);
            //IObjectMapper mpr = mapper.ConfigurationProvider.FindMapper(tp);


            return;
        }

        private void CheckRequiresEmitted(bool fromMappingConfig, Type destinationType)
        {
            IEnumerable<WasEmittedAttribute> list = destinationType.GetCustomAttributes(typeof(WasEmittedAttribute), true).Cast<WasEmittedAttribute>();
            WasEmittedAttribute f = list.FirstOrDefault();
            bool theWasEmittedAttributeWasFound = f != null;

            if(fromMappingConfig != theWasEmittedAttributeWasFound)
            {
                // TODO: Make the WasEmitted mismatch error more descriptive.
                throw new InvalidOperationException("AbstractPropBagMapper found a 'WasEmitted mismatch.");
            }
        }

        private TDestination GetDestinationTemplate(Type targetType)
        {
            TDestination result = GetNewDestination(DestinationType, PropModel, _viewModelFactory, _propBagMapperService, PropFactory, fullClassName: null);

            if (TestCreateDest(targetType, result))
            {
                // TODO: Consider removing the responsiblity of fixing the store's PropNodeCollection from the AbstractPropBagMapper class.
                if (!PropModel.IsFixed)
                {
                    PropModel.Fix();
                }

                if (!_viewModelFactory.PropStoreAccessServiceCreator.IsPropItemSetFixed(result))
                {
                    // Fix the template's PropNodeCollection to improve performance.
                    PropItemSetKeyType propItemSetKey = new PropItemSetKeyType(PropModel);
                    _viewModelFactory.PropStoreAccessServiceCreator.TryFixPropItemSet(result, propItemSetKey);
                }
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
                GetNewDestination(targetType, template, _viewModelFactory.ViewModelActivator);
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

            //object d = GetNewDestinationTest(RunTimeType, _destPropBagTemplate);
            //TDestination result = MapToDestination(s, d);

            return result;
        }

        public TDestination MapToDestination(TSource s, TDestination d)
        {
            return (TDestination) Mapper.Map(s, d, SourceType, TargetRunTimeType);
        }

        public TDestination MapToDestination(TSource s, object d)
        {
            return (TDestination)Mapper.Map(s, d, SourceType, TargetRunTimeType);
        }

        public TSource MapToSource(TDestination d)
        {
            return (TSource)Mapper.Map(d, TargetRunTimeType, SourceType);
        }

        public TSource MapToSource(TDestination d, TSource s)
        {
            return (TSource) Mapper.Map(d, s, TargetRunTimeType, SourceType);
        }

        #endregion

        #region Generic Mapper Functions

        public object MapToDestination(object source)
        {
            return Mapper.Map(source, GetNewDestination(), typeof(TSource), typeof(TDestination));
        }

        #endregion

        #region Typed Mapper Functions for Lists

        public IEnumerable<TDestination> MapToDestination(IEnumerable<TSource> listOfSources)
        {
            return listOfSources.Select(s => MapToDestination(s));
        }

        public IEnumerable<TSource> MapToSource(IEnumerable<TDestination> listOfDestinations)
        {
            return listOfDestinations.Select(d => MapToSource(d));
        }

        #endregion

        #region Generic Mapper Functions for Lists

        public IEnumerable<object> MapToDestination(IEnumerable<object> listOfSources)
        {
            return listOfSources.Select(s => MapToDestination(s));
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
                    result = GetNewDestination(TargetRunTimeType, _destPropBagTemplate, _viewModelFactory.ViewModelActivator);
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
                //result = GetNewDestination(RunTimeType, PropModel, _storeAccessCreator, _autoMapperService, _wrapperTypeCreator, PropFactory, fullClassName: null);
                result = GetNewDestination(TargetRunTimeType, PropModel, _viewModelFactory, _propBagMapperService, PropFactory, fullClassName: null);

                _mct.MeasureAndReport("GetNewDestination(PropModel, ...", "AstractPropBagMapper");
            }

            return result;
        }

        // Regular Instantiation using the PropModel. 
        //private TDestination GetNewDestination(Type destinationOrProxyType, PropModelType propModel, PSAccessServiceCreatorInterface storeAccessCreator, IProvideAutoMappers autoMapperService, ICreateWrapperTypes wrapperTypeCreator, IPropFactory propFactory, string fullClassName)

        private TDestination GetNewDestination(Type destinationOrProxyType, PropModelType propModel,
            ViewModelFactoryInterface viewModelFactory, IPropBagMapperService propBagMapperService,
            IPropFactory propFactory, string fullClassName)
        {
            ViewModelActivatorInterface vmActivator = viewModelFactory.ViewModelActivator;
            try
            {
                //var newViewModel =  _vmActivator.GetNewViewModel(destinationOrProxyType, propModel, viewModelFactory, propFactory, fullClassName);

                var newViewModel = vmActivator.GetNewViewModel(destinationOrProxyType, propModel, viewModelFactory, propBagMapperService, propFactory, fullClassName);

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
        private TDestination GetNewDestination(Type destinationOrProxyType, IPropBag copySource, ViewModelActivatorInterface vmActivator)
        {
            try
            {
                var newViewModel = vmActivator.GetNewViewModel(destinationOrProxyType, copySource);
                return newViewModel as TDestination;
            }
            catch (Exception e2)
            {
                throw new InvalidOperationException($"Cannot create an instance of {destinationOrProxyType} that takes a single IPropag argument.", e2);
            }
        }

        //private object GetNewDestinationTest(Type destinationOrProxyType, IPropBag copySource)
        //{
        //    try
        //    {
        //        var newViewModel = _vmActivator.GetNewViewModel(destinationOrProxyType, copySource);
        //        return newViewModel;
        //    }
        //    catch (Exception e2)
        //    {
        //        throw new InvalidOperationException($"Cannot create an instance of {destinationOrProxyType} that takes a single IPropag argument.", e2);
        //    }
        //}

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
