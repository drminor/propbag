using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface ICachePropModels<L2TRaw> 
    {
        long Add(IPropModel<L2TRaw> propModel);

        void Fix(IPropModel<L2TRaw> propModel);

        IPropModel<L2TRaw> Open(IPropModel<L2TRaw> propModel, out long generationId);
        IPropModel<L2TRaw> Open(IPropModel<L2TRaw> propModel, string fullClassName, out long generationId);


        bool TryFind(IPropModel<L2TRaw> propModel, out long generationId);
        bool TryGetAllGenerations(string fullClassName, out IReadOnlyDictionary<long, IPropModel<L2TRaw>> familyCollection);
        bool TryGetValue(string fullClassName, long generationId, out IPropModel<L2TRaw> propModel);

        // These are a hold over from the old way.
        IPropModel<L2TRaw> GetPropModel(string resourceKey);

        IMapperRequest GetMapperRequest(string resourceKey);

    }
}