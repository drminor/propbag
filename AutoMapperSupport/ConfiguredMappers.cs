using AutoMapper;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.AutoMapperSupport
{
    public class ConfiguredMappers
    {
        private bool _sealed;
        private LockingConcurrentDictionary<TypePair, IPropBagMapperGen> _propBagMappers;

        public ConfiguredMappers()
        {
            _propBagMappers = new LockingConcurrentDictionary<TypePair, IPropBagMapperGen>(GetPropBagMapper);
            _sealed = false;
        }

        public MapperConfiguration SealThis()
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (TypePair key in _propBagMappers.Keys)
                {
                    IPropBagMapperGen mapper = _propBagMappers[key];
                    mapper.Configure(cfg);
                }
            });

            config.AssertConfigurationIsValid();
            IMapper compositeMapDef = config.CreateMapper();
            _sealed = true;

            foreach (TypePair key in _propBagMappers.Keys)
            {
                IPropBagMapperGen mapper = _propBagMappers[key];
                mapper.Mapper = compositeMapDef;
            }

            return config;
        }

        public void AddMapReq(IPropBagMapperGen req)
        {
            if (_sealed) throw new ApplicationException("Cannot add additional mappers, this configuration is sealed.");

            _propBagMappers[req.TypePair] = req;
        }

        private IPropBagMapperGen GetPropBagMapper(TypePair types)
        {
            throw new ApplicationException("This is not supported.");
        }
    }
}
