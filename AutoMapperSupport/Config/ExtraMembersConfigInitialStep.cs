using AutoMapper;
using AutoMapper.ExtraMembers;
using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DRM.PropBag.AutoMapperSupport
{
    public class ExtraMembersConfigInitialStep : IMapperConfigurationStepGen
    {
        public Action<IMapperConfigurationExpression> ConfigurationStep
        {
            get
            {
                return BuildExtraMemberConfig;
            }
        }

        public void BuildExtraMemberConfig(IMapperConfigurationExpression cfg)
        {
            cfg.DefineExtraMemberGetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetGetterStrategy);
            cfg.DefineExtraMemberSetterBuilder(PropertyInfoWT.STRATEGEY_KEY, GetSetterStrategy);

            cfg.ShouldMapField = ShouldMap;
            cfg.ShouldMapProperty = ShouldMap;
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

        public bool ShouldMap(MemberInfo mi)
        {
            if (IsPublic(mi)) return true;

            Attribute[] atts = mi.GetCustomAttributes(true) as Attribute[];
            if (atts == null) return false;
            Attribute test = atts.FirstOrDefault(a => a is ExtraMemberAttribute);

            return test != null;
        }

        private bool IsPublic(MemberInfo mi)
        {
            if (mi is MethodInfo methInfo) return methInfo.IsPublic;
            if (mi is PropertyInfo pi) return pi.GetMethod.IsPublic;
            return false;
        }
    }
}

