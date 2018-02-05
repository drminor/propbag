using System;

namespace DRM.TypeSafePropertyBag
{
    using GenerationIdType = UInt32;

    internal interface ICacheLevel2KeyManagers<L2KeyManType, L2T, L2TRaw> where L2KeyManType : class, IL2KeyMan<L2T, L2TRaw>
    {
        //bool IsRegistered(L2KeyManType level2KeyManager, uint generationId, 
        //    out KeyValuePair<Tuple<L2KeyManType, uint>, L2KeyManType> kvp);

        //bool TryGetReferenceBase(L2KeyManType level2KeyManager, uint generationId,
        //    out L2KeyManType level2KeyManagerReferenceBase);

        bool TryGetValueAndGenerationId(L2KeyManType level2KeyManager, out L2KeyManType basePropItemSet, out GenerationIdType generationId);

        bool TryGetGeneration(L2KeyManType level2KeyManager, out GenerationIdType generationId);

        bool TryRegisterBaseL2KeyMan(L2KeyManType level2KeyManager);

        bool TryRegisterL2KeyMan(L2KeyManType level2KeyManager, L2KeyManType basePropItemSet, out uint generationId);
    }
}