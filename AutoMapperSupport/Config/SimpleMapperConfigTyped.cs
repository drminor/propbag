using System;
using System.Collections.Generic;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapperConfigTyped<TSource, TDestination>
        : AbstractMapperConfigGen, IConfigureAMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        public ICreateMappingExpressions<TSource, TDestination> FinalConfigActionProvider { get; }

        public Func<TDestination, TSource> SourceConstructor { get; }

        public Func<TSource, TDestination> DestinationConstructor { get; }

        #region Constructor

        public SimpleMapperConfigTyped
            (
                List<IHaveAMapperConfigurationStep> configSteps,
                ICreateMappingExpressions<TSource, TDestination> finalConfigActionProvider,
                Func<TDestination, TSource> sourceConstructor,
                Func<TSource, TDestination> destinationConstructor,
                IHaveAMapperConfigurationStep configStarter,
                bool requiresWrappperTypeEmitServices,
                bool supportsMapFrom = true
            )
            : base
            (
                  configSteps,
                  configStarter,
                  requiresWrappperTypeEmitServices,
                  supportsMapFrom)
        {
            FinalConfigActionProvider = finalConfigActionProvider;
            SourceConstructor = sourceConstructor;
            DestinationConstructor = destinationConstructor;
        }

        #endregion

    }
}
