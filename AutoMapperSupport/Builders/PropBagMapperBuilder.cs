﻿using AutoMapper;
using DRM.ViewModelTools;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class PropBagMapperBuilder<TSource, TDestination> : IBuildPropBagMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        private IConfigureAMapper<TSource, TDestination> MapperConfiguration { get; }

        private IViewModelActivator<TDestination> VmActivator { get; }

        public PropBagMapperBuilder(IConfigureAMapper<TSource, TDestination> mapperConfiguration, IViewModelActivator<TDestination> vmActivator)
        {
            MapperConfiguration = mapperConfiguration;
            VmActivator = vmActivator;
        }

        public Func<IPropBagMapperKeyGen, IPropBagMapperGen> GenMapperCreator => GenerateMapperGen;

        public IPropBagMapper<TSource, TDestination> GenerateMapper(IPropBagMapperKey<TSource, TDestination> mapRequest)
        {
            // TODO: Get IMapper from mapRequest.
            IMapper mm = null;

            IPropBagMapper<TSource, TDestination> result 
                = new SimplePropBagMapper<TSource, TDestination>(mapRequest, mm, VmActivator);

            return result;
        }

        private IPropBagMapperGen GenerateMapperGen(IPropBagMapperKeyGen mapRequestGen)
        {
            return (IPropBagMapperGen)GenerateMapper((IPropBagMapperKey<TSource, TDestination>)mapRequestGen);
        }
    }
}
