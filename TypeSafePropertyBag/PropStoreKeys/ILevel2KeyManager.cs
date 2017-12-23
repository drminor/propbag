
using System;

namespace DRM.TypeSafePropertyBag
{
    internal interface IL2KeyMan<L2T, L2TRaw> : IDisposable
    {
        int MaxPropsPerObject { get; }

        L2T FromRaw(L2TRaw rawBot);
        bool TryGetFromRaw(L2TRaw rawBot, out L2T bot);

        L2TRaw FromCooked(L2T bot);
        bool TryGetFromCooked(L2T raw, out L2TRaw rawBot);

        L2T Add(L2TRaw rawBot);
        L2T GetOrAdd(L2TRaw rawBot);

        int PropertyCount { get; }
    }
}
