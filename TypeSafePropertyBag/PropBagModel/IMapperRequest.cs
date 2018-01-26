﻿using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IMapperRequest
    {
        string ConfigPackageName { get; set; }
        IPropModel PropModel { get; set; }
        string PropModelResourceKey { get; set; }
        Type SourceType { get; set; }
    }
}