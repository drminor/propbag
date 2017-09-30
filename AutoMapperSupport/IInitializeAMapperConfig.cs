using AutoMapper;
using AutoMapper.Configuration;
using System;
using System.Linq.Expressions;
using System.Reflection;
using DRM.TypeSafePropertyBag;
using AutoMapper.ExtraMembers;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IInitializeAMapperConf
    {
        Action<IMapperConfigurationExpression> InitialConfigurationAction { get; }
    }




}
