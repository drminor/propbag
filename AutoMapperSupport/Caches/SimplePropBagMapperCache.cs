using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Runtime.Remoting.Contexts;

namespace DRM.PropBag.AutoMapperSupport
{
    //[Synchronization()]
    public class SimplePropBagMapperCache : /*ContextBoundObject,*/ ICachePropBagMappers
    {
        private int pCntr = 0;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen> _unSealedPropBagMappers;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen> _sealedPropBagMappers;

        public SimplePropBagMapperCache()
        {
            _unSealedPropBagMappers = 
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen>
                (GetPropBagMapperPromise);

            _sealedPropBagMappers =
                new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen>
                (GetPropBagMapperReal);
        }

        public void RegisterMapperRequest(IPropBagMapperKeyGen mapRequest)
        {
            if (!_sealedPropBagMappers.ContainsKey(mapRequest))
            {
                _unSealedPropBagMappers.GetOrAdd(mapRequest);
            }
        }

        public IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapRequest)
        {
            if (_sealedPropBagMappers.TryGetValue(mapRequest, out IPropBagMapperGen result))
            {
                return result;
            }

            _unSealedPropBagMappers.GetOrAdd(mapRequest);
            int numberInThisBatch = SealThis(pCntr++);
            result = _sealedPropBagMappers[mapRequest];

            return result;
        }

        public int SealThis(int cntr)
        {
            System.Diagnostics.Debug.WriteLine($"Creating Profile_{cntr.ToString()}");

            int result = 0;
            foreach (IPropBagMapperKeyGen key in _unSealedPropBagMappers.Keys)
            {
                IPropBagMapperGen mapper = _sealedPropBagMappers.GetOrAdd(key);
                if (!(_unSealedPropBagMappers.TryRemoveValue(key, out IPropBagMapperKeyGen dummyKey)))
                {
                    throw new ApplicationException("Couldn't remove mappper request from list of registered, pending to be created, mapper requests.");
                }
                result++;
            }
            return result;
        }

        private IPropBagMapperGen GetPropBagMapperReal(IPropBagMapperKeyGen key)
        {
            IPropBagMapperGen result = key.CreateMapper();
            return result;
        }

        private IPropBagMapperKeyGen GetPropBagMapperPromise(IPropBagMapperKeyGen key)
        {
            return key;
        }

        #region OLD
        //public int SealThis_OLD(int cntr)
        //{
        //    System.Diagnostics.Debug.WriteLine($"Creating Profile_{cntr.ToString()}");

        //    int result = 0;
        //    foreach (IPropBagMapperKeyGen key in _unSealedPropBagMappers.Keys)
        //    {
        //        IPropBagMapperGen mapper = _sealedPropBagMappers.GetOrAdd(key);
        //        if (!(_unSealedPropBagMappers.TryRemoveValue(key, out IPropBagMapperKeyGen dummyKey)))
        //        {
        //            throw new ApplicationException("Couldn't remove mappper request from list of registered, pending to be created, mapper requests.");
        //        }
        //        result++;
        //    }
        //    return result;

        //    //IConfigurationProvider config = _baseConfigBuilder(UseInitialConfigAndConfigureTheMappers);

        //    //// TODO: Handle this
        //    ////config.AssertConfigurationIsValid();

        //    //IMapper compositeMapper = config.CreateMapper();

        //    //ProvisionTheMappers(compositeMapper);

        //    //_unSealedPropBagMappers.Clear();

        //    //return config;
        //    //return null;
        //}

        //void UseInitialConfigAndConfigureTheMappers(IMapperConfigurationExpression cfg)
        //{
        //    _mapperConfigInitializerProvider.InitialConfigurationAction(cfg);
        //    ConfigureTheMappers(cfg);
        //}

        //void ConfigureTheMappers(IMapperConfigurationExpression cfg)
        //{
        //    foreach (IPropBagMapperKeyGen key in _unSealedPropBagMappers.Keys)
        //    {
        //        IPropBagMapperGen mapper = _sealedPropBagMappers.GetOrAdd(key);
        //        mapper.Configure(cfg);
        //    }
        //}

        //void ProvisionTheMappers(IMapper compositeMapper)
        //{
        //    foreach (IPropBagMapperKeyGen key in _sealedPropBagMappers.Keys)
        //    {
        //        IPropBagMapperGen mapper = _sealedPropBagMappers[key];
        //        if (mapper.Mapper == null)
        //        {
        //            mapper.Mapper = compositeMapper;
        //            System.Diagnostics.Debug.WriteLine($"Setting the mapper for Mapper with key = {key}.");
        //        }
        //        else
        //        {
        //            System.Diagnostics.Debug.WriteLine($"The Mapper with key = {key} already has it's mapper set.");
        //        }
        //    }
        //}
        #endregion

    }
}
