using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    internal interface ICacheLevel2KeyManagers<L2KeyManType, L2T, L2TRaw> where L2KeyManType : class, IL2KeyMan<L2T, L2TRaw>
    {
        bool IsRegistered(L2KeyManType level2KeyManager, uint generationId, 
            out KeyValuePair<Tuple<L2KeyManType, uint>, L2KeyManType> kvp);

        bool TryGetReferenceBase(L2KeyManType level2KeyManager, uint generationId,
            out L2KeyManType level2KeyManagerReferenceBase);

        bool TryRegisterBaseL2KeyMan(L2KeyManType level2KeyManager);

        bool TryRegisterL2KeyMan(L2KeyManType level2KeyManager, L2KeyManType level2KeyManagerReferenceBase,
            out uint generationId);
    }
}