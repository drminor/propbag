using System;
using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag
{
    public interface ITypeInfoField
    {
        ObservableCollection<ITypeInfoField> Children { get; set; }
        string FullyQualifiedTypeName { get; set; }
        Type MemberType { get; set; }
        string TypeName { get; set; }
        Type TypeParameter1 { get; set; }
        Type TypeParameter2 { get; set; }
        Type TypeParameter3 { get; set; }
        WellKnownCollectionTypeEnum? WellKnownCollectionType { get; set; }
    }
}