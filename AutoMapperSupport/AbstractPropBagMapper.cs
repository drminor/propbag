﻿using AutoMapper;
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

        IPropBagMapperKey<TSource, TDestination> _mapRequest;
        //private IMapTypeDefinitionGen SourceTypeDef;
        //private IMapTypeDefinitionGen DestinationTypeDef;

        public Type SourceType { get; }
        public Type DestinationType { get; }

        public PropModel PropModel { get; }
        public Type RunTimeType { get; }

        IPropFactory PropFactory { get; } //= new PropFactory(true, null, null);
        public IMapper Mapper { get; }

        //public Func<TDestination, TSource> RegularInstanceCreator { get; }

        public bool SupportsMapFrom { get; }

        IViewModelActivator _vmActivator;

        #endregion

        #region Constructor

        public AbstractPropBagMapper(IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapper mapper, IViewModelActivator vmActivator)
        {
            _mapRequest = mapRequest;

            SourceType = mapRequest.SourceTypeDef.Type;
            DestinationType = mapRequest.DestinationTypeDef.Type;
            RunTimeType = mapRequest.DestinationTypeDef.NewWrapperType; // TODO: Work on this!!

            PropModel = mapRequest.DestinationTypeDef.PropModel;
            PropFactory = mapRequest.DestinationTypeDef.PropFactory;

            Mapper = mapper;
            _vmActivator = vmActivator;

            SupportsMapFrom = true;
        }

        #endregion

        #region Type Mapper Function

        public TDestination MapToDestination(TSource s)
        {
            TDestination proxyViewModel = GetNewDestination(PropModel, PropFactory, RunTimeType);

            return (TDestination)Mapper.Map(s, proxyViewModel, SourceType, RunTimeType);
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
            //List<TDestination> result = new List<TDestination>();

            //foreach (TSource s in listOfSources)
            //{
            //    TDestination d = this.MapToDestination(s);
            //    result.Add(d);
            //}

            //return result;

            return listOfSources.Select(s => MapToDestination(s));
        }

        public IEnumerable<TSource> MapToSource(IEnumerable<TDestination> listOfDestinations)
        {
            //List<TSource> result = new List<TSource>();

            //foreach (TDestination d in listOfDestinations)
            //{
            //    TSource s = this.MapToSource(d);
            //    result.Add(s);
            //}

            //return result;
            return listOfDestinations.Select(d => MapToSource(d));
        }
        #endregion

        #region Create Instance of TDestination

        private TDestination GetNewDestination(PropModel propModel, IPropFactory propFactory, Type newWrappedType)
        {
            try
            {
                var newViewModel = _vmActivator.GetNewViewModel(propModel, propFactory, newWrappedType);
                return newViewModel as TDestination;
            }
            catch (System.Exception e2)
            {
                Type targetType = newWrappedType ?? typeof(TDestination);
                throw new InvalidOperationException($"Cannot create an instance of {targetType} that takes a PropModel parameter.", e2);
            }
        }

        #endregion



    }
}
