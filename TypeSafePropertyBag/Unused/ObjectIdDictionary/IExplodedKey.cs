
using System;

namespace DRM.ObjectIdDictionary
{
    public interface IExplodedKey<CompT, L1T, L2T> : IEquatable<IExplodedKey<CompT, L1T, L2T>>
    {
        bool isEmpty { get; }
        CompT CKey { get; }
        L1T Level1Key { get; }
        L2T Level2Key { get; }
        //object AccessToken { get; }

        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // Join and split composite key from L1 and L2.
        CompT Fuse(L1T top, L2T bot);
        L1T Explode(CompT cKey, out L2T bot);

        bool Verify(CompT cKey, L1T top);
        bool Verify(CompT cKey, L1T top, L2T bot);
    }
}
