using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Execution
{
    using System.Diagnostics;
    using static Internal.ExpressionFactory;
    using static System.Linq.Expressions.Expression;

    internal class PropertyMapFuncHelper
    {
        PropertyMap propertyMap;
        Expression destination;
        TypeMapPlanBuilder _parentBuilder;
        Type _sourceType;
        bool destinationisExtraMember;
        MemberInfo _destExtraMemberInfo;
        MemberExpression _destMember;

        public PropertyMapFuncHelper(PropertyMap propertyMap, Expression destination, TypeMapPlanBuilder parentBuilder)
        {
            this.propertyMap = propertyMap;
            this.destination = destination;
            _parentBuilder = parentBuilder;

            _sourceType = propertyMap.SourceMember.GetMemberType();
        }

        public Expression Getter
        {
            get
            {
                destinationisExtraMember = IsExtraMember(propertyMap.DestinationProperty);
                if (destinationisExtraMember)
                {
                    // Retreive the Property or Field Info from the ProfileMap for this extra member.
                    _destExtraMemberInfo = propertyMap.ExtraDestinationMember; // GetDestExtraFieldOrPropertyInfo(propertyMap);
                    return CreateGetterExpression(_destExtraMemberInfo, destination, _sourceType);
                }
                else
                {
                    _destMember = MakeMemberAccess(destination, propertyMap.DestinationProperty);
                    DebugHelpers.LogExpression(_destMember, "destMember");

                    if (propertyMap.DestinationProperty is PropertyInfo pi && pi.GetGetMethod(true) == null)
                        return Default(propertyMap.DestinationPropertyType);
                    else
                        return _destMember;
                }
            }
        }

        public Expression GetValueResolverExpr(ParameterExpression source, Expression getter)
        {
            bool srcIsProxyProperty = IsExtraMember(propertyMap.SourceMember);

            if (srcIsProxyProperty)
            {
                // Retreive the Property or Field Info from the ProfileMap for this extra member.
                MemberInfo srcExtraMemberInfo = propertyMap.ExtraSourceMember; //GetSrcExtraFieldOrPropertyInfo(propertyMap);
                return CreateGetterExpression(srcExtraMemberInfo, source, propertyMap.DestinationPropertyType);
            }
            else
            {
                return _parentBuilder.BuildValueResolverFunc(propertyMap, getter);
            }
        }

        public Expression GetMapperFunc(ParameterExpression propertyValue)
        {
            if (destinationisExtraMember)
            {
                Debug.Assert(_destExtraMemberInfo != null, "extraMemberInfo should not be null here, but it is.");

                if (_destExtraMemberInfo is PropertyInfo pi && pi.GetSetMethod(true) == null)
                {
                    //throw new ArgumentException("Extra Member must have a SetMethod defined.");
                    return propertyValue;
                }
                else
                {
                    return propertyMap.SourceType != propertyMap.DestinationPropertyType
                        ? CreateSetterExpression(_destExtraMemberInfo, destination, _sourceType, (ParameterExpression)ToType(propertyValue, propertyMap.DestinationPropertyType))
                        : CreateSetterExpression(_destExtraMemberInfo, destination, _sourceType, propertyValue);
                }
            }
            else if (propertyMap.DestinationProperty is FieldInfo)
            {
                Debug.Assert(_destMember != null, "destMember should not be null here, but it is.");

                return propertyMap.SourceType != propertyMap.DestinationPropertyType
                    ? Assign(_destMember, ToType(propertyValue, propertyMap.DestinationPropertyType))
                    : Assign(_destMember, propertyValue);
            }
            else
            {
                Debug.Assert(_destMember != null, "destMember should not be null here, but it is.");

                //var setter = ((PropertyInfo)propertyMap.DestinationProperty).GetSetMethod(true);
                //if (setter == null)
                if (propertyMap.DestinationProperty is PropertyInfo pi && pi.GetSetMethod(true) == null)
                    return propertyValue;
                else
                    return Assign(_destMember, ToType(propertyValue, propertyMap.DestinationPropertyType));
            }
        }

        private Expression CreateGetterExpression(MemberInfo mi, Expression destination, Type sourceType)
        {
            Expression[] parameters = new Expression[3] { Expression.Constant(mi), destination, Expression.Constant(sourceType) };
            MethodInfo theGetMethod = AutoMapper.Internal.ReflectionHelper.GetPropertyValueMethod;
            MethodCallExpression callExpr = Expression.Call(theGetMethod, parameters);

            Expression cast = ToType(callExpr, mi.GetMemberType());
            return cast;
        }

        private MethodCallExpression CreateSetterExpression(MemberInfo mi, Expression destination, Type sourceType, ParameterExpression value)
        {
            Expression[] parameters;
            if (mi is PropertyInfo pi && pi.PropertyType.IsValueType())
            {
                var cast = Expression.TypeAs(value, typeof(object));
                parameters = new Expression[4] { Expression.Constant(mi), destination, Expression.Constant(sourceType), cast };
            }
            else
            {
                parameters = new Expression[4] { Expression.Constant(mi), destination, Expression.Constant(sourceType), value };
            }

            MethodInfo theSetMethod = AutoMapper.Internal.ReflectionHelper.SetPropertyValueMethod;
            MethodCallExpression callExpr = Expression.Call(theSetMethod, parameters);
            return callExpr;
        }

        private bool IsExtraMember(MemberInfo mi)
        {
            if (mi is PropertyInfo pi)
            {
                return pi.GetCustomAttribute<ExtraMemberAttribute>(true) != null;
            }
            else if (mi is FieldInfo fi)
            {
                return fi.GetCustomAttribute<ExtraMemberAttribute>(true) != null;
            }
            return false;
        }
    }
}