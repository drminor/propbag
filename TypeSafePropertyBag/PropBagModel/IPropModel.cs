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
        bool DeferMethodRefResolution { get; set; } // Probably will not be used.

        DeriveFromClassModeEnum DeriveFromClassMode { get; set; }
        ObservableCollection<string> Namespaces { get; set; }

        /// <summary>
        /// If the DeriveFromClassMode = 'Custom'. This is the Custom Type.
        /// </summary>
        Type TypeToWrap { get; }

        // Currently not used. Will be used to hold information for generic type parameters and additional
        // dependent arguments that may need to be supplied when activating the custom target type.
        ITypeInfoField WrapperTypeInfoField { get; set; }

        /// <summary>
        /// The emitted type used for this PropModel, if an emitted type has been generated, otherwise null.
        /// </summary>
        Type NewEmittedType { get; set; }

        /// <summary>
        /// The destination type. 
        /// This will be same value as NewEmittedType if NewEmitedType is not null,
        /// Otherwise will be 'PropBag', 'PubPropBag' or the value of TypeToWrap, 
        /// depending on the value of DeriveFromClassMode.
        /// </summary>
        Type TargetType { get; }

        IPropFactory PropFactory { get; set; }
        Type PropFactoryType { get; set; }

        string FullClassName { get; } // Provides Canonical version of the Namespace + Class Name

        /// <summary>
        /// Used to associate this with a particular cache instance and is used,
        /// if present, to Fix and Open this PropModel.
        /// </summary>
        ICachePropModels<L2TRaw> PropModelCache { get; set; }

        bool IsFixed { get; }
        void Fix();
        void Open();

        /// <summary>
        /// If associated with a cache instance, this will refer to the fixed PropModel 
        /// from which this PropModel was first opened from.
        /// </summary>
        IPropModel<L2TRaw> Parent { get; set; }

        /// <summary>
        /// Each PropModel must have a unique combination of FullClassName and GenerationId.
        /// </summary>
        long GenerationId { get; set; }

        IPropModel<L2TRaw> CloneIt();
        IPropModel<L2TRaw> CloneIt(long generationId);

        ICustomTypeDescriptor CustomTypeDescriptor { get; set; }
    }
}