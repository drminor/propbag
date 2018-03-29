﻿using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;

    public interface IMapTypeDefinitionProvider
    {
        IMapTypeDefinition<T> GetTypeDescription<T>
            (
            PropModelType propModel,
            //Type typeToWrap,
            IPropFactory propFactory,
            string className
            );
    }
}
