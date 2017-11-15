
namespace DRM.TypeSafePropertyBag
{
    public interface ICKeyMan<CompT, L1T, L2T, L2TRaw> 
    {
        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        // Try version of Join Comp
        //bool TryJoinComp(L1T top, L2TRaw rawBot, out CompT cKey, out L2T bot);

        // Join and split composite key from L1 and L2.
        CompT JoinComp(L1T top, L2T bot);
        L1T SplitComp(CompT cKey, out L2T bot);

        // Join and split composite key from L1 and L2Raw.
        //CompT JoinComp(L1T top, L2TRaw rawBot);
        //L1T SplitComp(CompT cKey, out L2TRaw rawBot);

        bool Verify(CompT cKey, L1T top);

        bool Verify(CompT cKey, L1T top, L2T bot);

        //bool Verify(CompT cKey, L1T top, L2TRaw rawBot);
    }
}
