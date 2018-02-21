using System;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = Int64;

    internal interface ICachePropItemSets<PropItemSetType, L2T, L2TRaw> where PropItemSetType : class, IPropNodeCollection_Internal<L2T, L2TRaw>
    {
        bool TryGetValueAndGenerationId(PropItemSetType propItemSet, out PropItemSetType basePropItemSet, out GenerationIdType generationId);

        bool TryGetGeneration(PropItemSetType propItemSet, out GenerationIdType generationId);

        bool TryRegisterBasePropItemSet(PropItemSetType propItemSet);

        bool TryRegisterPropItemSet(PropItemSetType propItemSet, PropItemSetType basePropItemSet, out GenerationIdType generationId);
    }
}