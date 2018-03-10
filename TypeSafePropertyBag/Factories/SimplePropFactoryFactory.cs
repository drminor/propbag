using System;

namespace DRM.TypeSafePropertyBag
{
    public class SimplePropFactoryFactory : IPropFactoryFactory
    {
        private readonly IProvideDelegateCaches _delegateCacheProvider;
        private readonly IConvertValues _valueConverter;
        private readonly ResolveTypeDelegate _typeResolver;

        private SimplePropFactoryFactory()
        {
            throw new NotSupportedException($"Use of the parameterless constructor to create a {nameof(SimplePropFactoryFactory)} is not supported.");
        }

        public SimplePropFactoryFactory(IProvideDelegateCaches delegateCacheProvider, IConvertValues valueConverter)
            : this(delegateCacheProvider, valueConverter, null)
        {
        }

        public SimplePropFactoryFactory(IProvideDelegateCaches delegateCacheProvider, IConvertValues valueConverter, ResolveTypeDelegate typeResolver)
        {
            _delegateCacheProvider = delegateCacheProvider;
            _valueConverter = valueConverter;
            _typeResolver = typeResolver;
        }

        public IPropFactory BuildPropFactory(Type typeToActivate)
        {
            IPropFactory propFactory = (IPropFactory) Activator.CreateInstance(typeToActivate, _delegateCacheProvider, _valueConverter, _typeResolver);

            return propFactory;
        }


    }
}
