using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropItemSetInterface = IPropItemSet<String>;

    public interface IPropModel
    {
        string ClassName { get; set; }
        string NamespaceName { get; set; }
        PropBagTypeSafetyMode TypeSafetyMode { get; set; }
        bool RequireExplicitInitialValue { get; set; }

        ObservableCollection<IPropModelItem> Props { get; set; }

        DeriveFromClassModeEnum DeriveFromClassMode { get; set; }
        ObservableCollection<string> Namespaces { get; set; }
        Type TargetType { get; set; }
        Type TypeToCreate { get; }
        ITypeInfoField WrapperTypeInfoField { get; set; }

        IPropFactory PropFactory { get; set; } // Is being phased out.
        Type PropFactoryType { get; set; }

        bool DeferMethodRefResolution { get; set; } // Probably will not be used.

        string FullClassName { get; } // Provides Canonical version of the Namespace + Class Name

        IProvidePropModels PropModelProvider { get; set; }

        PropItemSetInterface PropItemSet { get; set; }

        PropertyDescriptorCollection PropertyDescriptorCollection { get; set; }
    }
}