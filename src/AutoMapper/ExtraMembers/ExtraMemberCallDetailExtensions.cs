using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AutoMapper.ExtraMembers
{
    public static class ExtraMemberCallDetailExtensions
    {
        public static MethodInfo GetMethod(this ExtraMemberCallDetails details)
        {
            if(details.CallDirection == ExtraMemberCallDirectionEnum.Get)
            {
                if(details.CallType == ExtraMemberCallTypeEnum.PropertySimpleArgs)
                    return AutoMapper.Internal.ReflectionHelper.GetPropertyValueMethod;
            }
            else
            {
                if(details.CallType == ExtraMemberCallTypeEnum.PropertySimpleArgs)
                {
                    return AutoMapper.Internal.ReflectionHelper.SetPropertyValueMethod;
                }
            }
            throw new ArgumentOutOfRangeException("ExtraMemberCallDetails has a non-supported CallType / Call Direction combination.");
            
        }
    }
}
