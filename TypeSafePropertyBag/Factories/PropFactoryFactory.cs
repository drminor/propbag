using System;

namespace DRM.TypeSafePropertyBag
{
    public class PropFactoryFactory : IPropFactoryFactory
    {
        private readonly IProvideDelegateCaches _delegateCacheProvider;
        private readonly IConvertValues _valueConverter;
        private readonly ResolveTypeDelegate _typeResolver;

        private PropFactoryFactory()
        {
        }

        public PropFactoryFactory(IProvideDelegateCaches delegateCacheProvider, IConvertValues valueConverter)
            : this(delegateCacheProvider, valueConverter, null)
        {
        }

        public PropFactoryFactory(IProvideDelegateCaches delegateCacheProvider, IConvertValues valueConverter, ResolveTypeDelegate typeResolver)
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
