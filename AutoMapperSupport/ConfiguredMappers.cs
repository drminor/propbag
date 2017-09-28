using AutoMapper;
using AutoMapper.Configuration;
using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ConfiguredMappers
    {
        private int pCntr = 0;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen> _unSealedPropBagMappers;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen> _sealedPropBagMappers;

        private Func<MapperConfigurationExpression, MapperConfiguration> _baseConfigBuilder;
        private IMapperStrategyConfigExpProvider _initialMapperConfigProvider;

        public ConfiguredMappers(Func<MapperConfigurationExpression, MapperConfiguration> baseConfigBuilder,
            IMapperStrategyConfigExpProvider initialConfigProvider)
        {
            _baseConfigBuilder = baseConfigBuilder;
            _initialMapperConfigProvider = initialConfigProvider;
            _unSealedPropBagMappers = new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen>(GetPropBagMapperPromise);
            _sealedPropBagMappers = new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen>(GetPropBagMapperReal);
        }

        // TODO: Need to use a lock here, if one has not already been aquired by GetMapperToUse.
        public MapperConfiguration SealThis(int cntr)
        {
            MapperConfigurationExpression mce = _initialMapperConfigProvider.InitialConfiguration;

            mce.AddMemberConfiguration();

            ConfigureTheMappers(mce);

            System.Diagnostics.Debug.WriteLine($"Creating Profile_{cntr.ToString()}");

            // This next line creates a ProfileConfiguration which can be used like a profile,
            // it does not, of couse, actaully create a profile.
            //mce.CreateProfile($"Profile_{cntr.ToString()}", f => { });

            MapperConfiguration config = _baseConfigBuilder(mce);

            // TODO: Handle this
            //config.AssertConfigurationIsValid();

            IMapper compositeMapper = config.CreateMapper();

            ProvisionTheMappers(compositeMapper);

            _unSealedPropBagMappers.Clear();

            return config;
        }

        void ConfigureTheMappers(MapperConfigurationExpression cfg)
        {
            foreach (IPropBagMapperKeyGen key in _unSealedPropBagMappers.Keys)
            {
                IPropBagMapperGen mapper =_sealedPropBagMappers.GetOrAdd(key);
                mapper.Configure(cfg);
            }
        }

        void ProvisionTheMappers(IMapper compositeMapper)
        {
            foreach (IPropBagMapperKeyGen key in _sealedPropBagMappers.Keys)
            {
                IPropBagMapperGen mapper = _sealedPropBagMappers[key];
                if (mapper.Mapper == null)
                {
                    mapper.Mapper = compositeMapper;
                    System.Diagnostics.Debug.WriteLine($"Setting the mapper for Mapper with key = {key}.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"The Mapper with key = {key} already has it's mapper set.");
                }
            }
        }

        // TODO: Need to aquire a lock here.
        public void Register(IPropBagMapperKeyGen mapRequest)
        {
            if (!_sealedPropBagMappers.ContainsKey(mapRequest))
            {
                _unSealedPropBagMappers.GetOrAdd(mapRequest);
            }
        }

        // TODO: Need to protect this with a lock.
        public IPropBagMapperGen GetMapperToUse(IPropBagMapperKeyGen mapRequest)
        {
            System.Diagnostics.Debug.WriteLine($"");
            IPropBagMapperGen result;
            if (_sealedPropBagMappers.TryGetValue(mapRequest, out result))
            {
                return result;
            }

            _unSealedPropBagMappers.GetOrAdd(mapRequest);
            SealThis(pCntr++);
            result = _sealedPropBagMappers[mapRequest];

            return result;
        }

        private IPropBagMapperGen GetPropBagMapperReal(IPropBagMapperKeyGen key)
        {
            return key.CreateMapper(key);
        }

        private IPropBagMapperKeyGen GetPropBagMapperPromise(IPropBagMapperKeyGen key)
        {
            return key;
        }
    }
}
