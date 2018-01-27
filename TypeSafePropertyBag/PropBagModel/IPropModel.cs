using System;
using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropModel
    {
        string ClassName { get; set; }
        bool DeferMethodRefResolution { get; set; }
        DeriveFromClassModeEnum DeriveFromClassMode { get; set; }
        string FullClassName { get; }
        string NamespaceName { get; set; }
        ObservableCollection<string> Namespaces { get; set; }
        IPropFactory PropFactory { get; set; }
        ObservableCollection<IPropItem> Props { get; set; }
        Type PropStoreServiceProviderType { get; set; }
        bool RequireExplicitInitialValue { get; set; }
        Type TargetType { get; set; }
        PropBagTypeSafetyMode TypeSafetyMode { get; set; }
        Type TypeToCreate { get; }
        ITypeInfoField WrapperTypeInfoField { get; set; }
    }
}