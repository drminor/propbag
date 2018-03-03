using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropModelFamilyCollection<L2TRaw>
    {
        long Add(IPropModel<L2TRaw> propModel);
        bool Contains(long generationId);
        long Find(IPropModel<string> propModel);

        IReadOnlyDictionary<long, IPropModel<L2TRaw>> GetAll();
        bool TryGetPropModel(long generationId, out IPropModel<string> propModel);
        bool TryGetPropModel(out IPropModel<string> propModel);
    }
}