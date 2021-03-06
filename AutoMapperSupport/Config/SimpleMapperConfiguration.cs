﻿using System;
using System.Collections.Generic;

namespace Swhp.AutoMapperSupport
{
    public class SimpleMapperConfiguration<TSource, TDestination>
        :
        AbstractMapperConfigurationGen,
        IConfigureAMapper<TSource, TDestination>
    {
        public IMapperConfigurationExpressionProvider<TSource, TDestination> FinalConfigActionProvider { get; }

        public Func<TDestination, TSource> SourceConstructor { get; }

        public Func<TSource, TDestination> DestinationConstructor { get; }

        #region Constructor

        public SimpleMapperConfiguration
            (
            string packageName,
            List<IHaveAMapperConfigurationStep> configSteps,
            IMapperConfigurationExpressionProvider<TSource, TDestination> finalConfigActionProvider,
            Func<TDestination, TSource> sourceConstructor,
            Func<TSource, TDestination> destinationConstructor,
            IHaveAMapperConfigurationStep configStarter,
            bool requiresWrappperTypeEmitServices,
            bool supportsMapFrom
            )
        : base
            (
            packageName,
            configSteps,
            configStarter,
            requiresWrappperTypeEmitServices,
            supportsMapFrom
            )
        {
            FinalConfigActionProvider = finalConfigActionProvider;
            SourceConstructor = sourceConstructor;
            DestinationConstructor = destinationConstructor;
        }

        #endregion
    }
}
