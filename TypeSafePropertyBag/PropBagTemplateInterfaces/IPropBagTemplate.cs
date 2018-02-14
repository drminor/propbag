using System;

using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropBagTemplate
    {
        string ClassName { get; set; }
        string OutPutNameSpace { get; set; }

        ObservableCollection<INamespaceItem> Namespaces { get; }
        ObservableCollection<IPropTemplateItem> Props { get; }

        PropBagTypeSafetyMode TypeSafetyMode { get; set; }

        Type TargetType { get; set; }
        DeriveFromClassModeEnum DeriveFromClassMode { get; set; }
        bool RequireExplicitInitialValue { get; set; }
        bool DeferMethodRefResolution { get; set; } // TODO: Remove this -- it's no longer being used.
        Type PropFactoryType { get; set; }

        string FullClassName { get; } // Convenience method that combines the ClassName and the OutputNameSpace.
    }
}