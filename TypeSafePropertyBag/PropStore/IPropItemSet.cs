using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropItemSet<L2TRaw>
    {
        //int MaxPropsPerObject { get; }

        //bool IsFixed { get; }
        //void Fix();

        int Count { get; }

        void Add(L2TRaw propertyName, IPropTemplate propTemplate);

        bool Contains(L2TRaw propertyName);
        bool Contains(IPropTemplate propTemplate);

        IEnumerable<L2TRaw> GetPropNames();
        IEnumerable<IPropTemplate> GetPropTemplates();

        bool TryGetPropTemplate(L2TRaw propertyName, out IPropTemplate propTemplate);
        bool TryRemove(L2TRaw propertyName, out IPropTemplate propTemplate);

        void Clear();
    }
}