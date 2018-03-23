using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface ICachePropModels<L2TRaw> 
    {
        long Add(IPropModel<L2TRaw> propModel);

        bool TryClone(IPropModel<L2TRaw> propModel, out IPropModel<L2TRaw> clonedCopy);

        bool TryGetPropModel(string fullClassName, out IPropModel<L2TRaw> propModel);
        bool TryGetPropModel(string fullClassName, long generationId, out IPropModel<L2TRaw> propModel);

        bool TryFix(IPropModel<L2TRaw> propModel);

        IPropModel<L2TRaw> Open(IPropModel<L2TRaw> propModel, out long generationId);
        IPropModel<L2TRaw> Open(IPropModel<L2TRaw> propModel, string fullClassName, out long generationId);

        bool TryFind(IPropModel<L2TRaw> propModel, out long generationId);
        bool TryGetAllGenerations(string fullClassName, out IReadOnlyDictionary<long, IPropModel<L2TRaw>> familyCollection);

        void Clear();

        //IPropModel<L2TRaw> GetPropModel(string resourceKey);

        IMapperRequest GetMapperRequest(string resourceKey);
    }
}