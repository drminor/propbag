using System;

namespace DRM.TypeSafePropertyBag
{
    public interface ICreateLocalBindings<PropDataT> where PropDataT : IPropGen
    {
        //Action<SourceT, SourceT> CreateLBinding<SourceT, TargetT>
        //    (SimpleExKey source, SimpleExKey target, LocalBindingInfo lbInfo);

        Action<SourceT, SourceT> CreateLBinding<SourceT>
            (SimpleExKey source, SimpleExKey target, LocalBindingInfo lbInfo);
    }
}