using AutoMapper;
using AutoMapper.ExtraMembers;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
                cfg.DefineExtraMemberGetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetGetterStrategy);

                cfg.DefineExtraMemberSetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetSetterStrategy);

                foreach (TypePair key in _propBagMappers.Keys)
                {
                    IPropBagMapperGen mapper = _propBagMappers[key];
                    mapper.Configure(cfg);
                }
            });

            // TODO: Handle this
            //config.AssertConfigurationIsValid();

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
