using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropModel<L2TRaw> : IPropItemSet<L2TRaw>, ICloneable
    {
        string ClassName { get; set; }
        string NamespaceName { get; set; }
        PropBagTypeSafetyMode TypeSafetyMode { get; set; }
        bool RequireExplicitInitialValue { get; set; }

        //ObservableCollection<IPropModelItem> Props { get; set; }

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

        bool IsFixed { get; }
        void Fix();

        IPropModel<L2TRaw> Parent { get; set; }
        long GenerationId { get; set; }

        PropertyDescriptorCollection PropertyDescriptorCollection { get; set; }
    }
}