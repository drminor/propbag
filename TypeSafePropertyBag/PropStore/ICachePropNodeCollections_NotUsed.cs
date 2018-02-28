using System;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = Int64;

    internal interface ICachePropNodeCollections<PropNodeCollectionInterface, L2T, L2TRaw> where PropNodeCollectionInterface : class, IPropNodeCollection_Internal<L2T, L2TRaw>
    {
        bool TryGetValueAndGenerationId(PropNodeCollectionInterface propItemSet, out PropNodeCollectionInterface basePropItemSet, out GenerationIdType generationId);

        bool TryGetGeneration(PropNodeCollectionInterface propItemSet, out GenerationIdType generationId);

        bool TryRegisterBasePropItemSet(PropNodeCollectionInterface propItemSet);

        bool TryRegisterPropItemSet(PropNodeCollectionInterface propItemSet, PropNodeCollectionInterface basePropItemSet, out GenerationIdType generationId);
    }
}