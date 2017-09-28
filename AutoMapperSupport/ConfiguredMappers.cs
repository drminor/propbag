using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.ExtraMembers;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ConfiguredMappers
    {
        private int pCntr = 0;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen> _unSealedPropBagMappers;
        private LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen> _sealedPropBagMappers;

        private Func<MapperConfigurationExpression, MapperConfiguration> _baseConfigBuilder;

        public ConfiguredMappers(Func<MapperConfigurationExpression, MapperConfiguration> baseConfigBuilder)
        {
            _baseConfigBuilder = baseConfigBuilder;
            _unSealedPropBagMappers = new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperKeyGen>(GetPropBagMapperPromise);
            _sealedPropBagMappers = new LockingConcurrentDictionary<IPropBagMapperKeyGen, IPropBagMapperGen>(GetPropBagMapperReal);
        }

        // TODO: Need to use a lock here, if one has not already been aquired by GetMapperToUse.
        public MapperConfiguration SealThis(int cntr)
        {
            MapperConfigurationExpression mce = new MapperConfigurationExpression();

            //mce.DefineExtraMemberGetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetGetterStrategy);
            //mce.DefineExtraMemberSetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetSetterStrategy);

            ConfigureTheMappers(mce);

            System.Diagnostics.Debug.WriteLine($"Creating Profile_{cntr.ToString()}");
            mce.CreateProfile($"Profile_{cntr.ToString()}", f => { });
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
                mapper.Mapper = compositeMapper;
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

        /// <summary>
        /// This assumes that mi will always be a PropertyInfo.
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="sourceType"></param> 
        /// <returns></returns>
        public ExtraMemberCallDetails GetGetterStrategy(MemberInfo mi, Expression destination, Type sourceType, IPropertyMap propertyMap)
        {
            Expression indexExp = Expression.Constant(new object[] { sourceType });

            Expression[] parameters = new Expression[3] { Expression.Constant(mi), destination, indexExp };
            return new ExtraMemberCallDetails(ExtraMemberCallDirectionEnum.Get, mi, parameters);
        }

        // This assumes that mi will always be a PropertyInfo.
        public ExtraMemberCallDetails GetSetterStrategy(MemberInfo mi, Expression destination, Type sourceType, IPropertyMap propertyMap, ParameterExpression value)
        {
            Expression newValue;
            if (mi is PropertyInfo pi && pi.PropertyType.IsValueType())
            {
                newValue = Expression.TypeAs((Expression)value, typeof(object));
            }
            else
            {
                newValue = value;
            }

            Expression indexExp = Expression.Constant(new object[] { propertyMap.SourceType });

            Expression[] parameters = new Expression[4] { Expression.Constant(mi), destination, newValue, indexExp };
            return new ExtraMemberCallDetails(ExtraMemberCallDirectionEnum.Set, mi, parameters);
        }
    }
}
