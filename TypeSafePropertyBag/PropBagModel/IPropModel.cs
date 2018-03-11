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

        /// <summary>
        /// The Type To Wrap if DeriveFromClassMode = custom
        /// </summary>
        Type TargetType { get; set; }

        /// <summary>
        /// PropBag, PubPropBag, or TargetType depending on the DeriveFromClassMode.
        /// </summary>
        Type TypeToCreate { get; }

        /// <summary>
        /// The emitted type used for this PropModel, if an emitted type has been generated, otherwise null.
        /// </summary>
        Type NewEmittedType { get; set; }

        ITypeInfoField WrapperTypeInfoField { get; set; }

        IPropFactory PropFactory { get; set; } // Is being phased out.
        Type PropFactoryType { get; set; }

        bool DeferMethodRefResolution { get; set; } // Probably will not be used.

        string FullClassName { get; } // Provides Canonical version of the Namespace + Class Name

        ICachePropModels<L2TRaw> PropModelCache { get; set; }
        //IProvidePropModels PropModelProvider { get; set; }

        bool IsFixed { get; }
        void Fix();
        void Open();

        IPropModel<L2TRaw> Parent { get; set; }
        long GenerationId { get; set; }

        //PropertyDescriptorCollection PropertyDescriptorCollection { get; set; }

        //ICustomTypeDescriptor CustomTypeDescriptor { get; set; }
        object TypeDescriptionProvider { get; set; }
    }
}