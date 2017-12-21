using AutoMapper;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using DRM.ViewModelTools;
using System;

using System.Collections.Generic;
using System.Linq;

namespace DRM.PropBag.AutoMapperSupport
{
    public abstract class AbstractPropBagMapper<TSource, TDestination> 
        : IPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        #region Public and Private Members

        public Type SourceType { get; }
        public Type DestinationType { get; }

        public PropModel PropModel { get; }
        public Type RunTimeType { get; }

        IPropFactory PropFactory { get; }
        public IMapper Mapper { get; }

        //public Func<TDestination, TSource> RegularInstanceCreator { get; }
        public bool SupportsMapFrom { get; }

        IViewModelActivator _vmActivator;

        private readonly bool _requiresWrappperTypeEmitServices;
        private readonly ICloneable _template;
        private readonly IPropBag _pbTemplate;

        #endregion

        #region Constructor

        public AbstractPropBagMapper(IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper, IViewModelActivator vmActivator)
        {
            SourceType = mapRequest.SourceTypeDef.TargetType;
            DestinationType = mapRequest.DestinationTypeDef.TargetType;

            RunTimeType = mapRequest.DestinationTypeDef.NewWrapperType ?? DestinationType; 
            PropModel = mapRequest.DestinationTypeDef.PropModel;
            PropFactory = mapRequest.DestinationTypeDef.PropFactory;

            Mapper = mapper;
            _vmActivator = vmActivator;

            SupportsMapFrom = true;

            _requiresWrappperTypeEmitServices = mapRequest.MappingConfiguration.RequiresWrappperTypeEmitServices;

            if (typeof(TDestination) is ICloneable)
            {
                if(_requiresWrappperTypeEmitServices)
                {
                    _template = null;
                    _pbTemplate = (IPropBag)GetNewDestination(PropModel, DestinationType, PropFactory, fullClassName: null);
                }
                else
                {
                    _template = (ICloneable)GetNewDestination(PropModel, DestinationType, PropFactory, fullClassName: null);
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
                result = GetNewDestination(PropModel, RunTimeType, PropFactory, fullClassName: null);
            }

            //result = GetNewDestination(PropModel, RunTimeType, PropFactory, fullClassName: null);
            return result;
        }

        private TDestination GetNewDestination(PropModel propModel, Type destinationTypeOrProxy, IPropFactory propFactory, string fullClassName)
        {
            try
            {
                var newViewModel = _vmActivator.GetNewViewModel(propModel, destinationTypeOrProxy, fullClassName, propFactory);
                return newViewModel as TDestination;
            }
            catch (Exception e2)
            {
                Type targetType = destinationTypeOrProxy ?? typeof(TDestination);
                throw new InvalidOperationException($"Cannot create an instance of {targetType} that takes a PropModel parameter.", e2);
            }
        }

        private TDestination GetNewDestination(Type destinationTypeOrProxy, IPropBag copySource)
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
    }
}
