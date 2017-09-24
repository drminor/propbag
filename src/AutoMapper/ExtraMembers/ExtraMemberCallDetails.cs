using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace AutoMapper.ExtraMembers
{
    public class ExtraMemberCallDetails
    {
        public ExtraMemberCallDirectionEnum CallDirection { get; private set; }
        public ExtraMemberCallTypeEnum CallType { get; private set; }
        public Expression[] Parameters { get; private set; }

        public ExtraMemberCallDetails()
        {
            CallDirection = ExtraMemberCallDirectionEnum.Get;
            CallType = ExtraMemberCallTypeEnum.NotSet;
            Parameters = new Expression[0];
        }

        public ExtraMemberCallDetails(ExtraMemberCallDirectionEnum callDirection,
            ExtraMemberCallTypeEnum callType,
            Expression[] parameters) 
        {
            CallDirection = callDirection;
            CallType = callType;
            Parameters = parameters;
        }

        public ExtraMemberCallDetails(ExtraMemberCallDirectionEnum callDirection,
            MemberInfo member,
            Expression[] parameters)
        {
            CallDirection = callDirection;
            CallType = GetCallType(callDirection, member);
            Parameters = parameters;
        }

        private static ExtraMemberCallTypeEnum GetCallType(ExtraMemberCallDirectionEnum callDirection,
            MemberInfo member)
        {

            return ExtraMemberCallTypeEnum.PropertySimpleArgs;
        }

        public static ExtraMemberCallDetails GetDefaultGetterDetails(MemberInfo member, Expression destination)
        {
            ExtraMemberCallTypeEnum callType = GetCallType(ExtraMemberCallDirectionEnum.Get, member);

            object[] index = new object[0];

            Expression indexExp = Expression.Constant(index);

            Expression[] parameters = new Expression[3] { Expression.Constant(member), destination, indexExp };

            return new ExtraMemberCallDetails(ExtraMemberCallDirectionEnum.Get, callType, parameters);
        }

        public static ExtraMemberCallDetails GetDefaultSetterDetails(MemberInfo member, Expression destination, ParameterExpression value)
        {
            ExtraMemberCallTypeEnum callType = GetCallType(ExtraMemberCallDirectionEnum.Set, member);

            object[] index = new object[0];

            Expression indexExp = Expression.Constant(index);

            // Value types cannot be implicitly converted to object,
            // we need to box the value if it's a valueType.
            Expression rVal;
            if(member.GetMemberType().IsValueType())
            {
                rVal = Expression.Convert(value, typeof(object));
            }
            else
            {
                rVal = value;
            }

            Expression[] parameters = new Expression[4] { Expression.Constant(member), destination, rVal, indexExp };

            return new ExtraMemberCallDetails(ExtraMemberCallDirectionEnum.Set, callType, parameters);
        }

    }
}
