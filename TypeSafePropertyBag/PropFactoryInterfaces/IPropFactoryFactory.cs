using System;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropFactoryFactory
    {
        IPropFactory BuildPropFactory(Type typeToActivate);
    }
}