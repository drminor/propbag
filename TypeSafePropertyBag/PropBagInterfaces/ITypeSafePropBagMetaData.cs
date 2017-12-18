namespace DRM.TypeSafePropertyBag
{
    public interface ITypeSafePropBagMetaData
    {
        bool AllPropsMustBeRegistered { get; }
        string ClassName { get; }
        string FullClassName { get; }
        bool OnlyTypedAccess { get; }
        ReadMissingPropPolicyEnum ReadMissingPropPolicy { get; }
        bool ReturnDefaultForUndefined { get; }
        PropBagTypeSafetyMode TypeSafetyMode { get; }
    }
}