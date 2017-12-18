
namespace DRM.ObjectIdDictionary
{
    public interface ICKeyMan<CompT, L1T, L2T, L2TRaw> 
    {
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // Join and split composite key from L1 and L2.
        CompT JoinComp(L1T top, L2T bot);
        L1T SplitComp(CompT cKey, out L2T bot);

        bool Verify(CompT cKey, L1T top);
        bool Verify(CompT cKey, L1T top, L2T bot);
    }
}
