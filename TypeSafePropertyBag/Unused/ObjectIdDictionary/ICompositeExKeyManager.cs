
namespace DRM.ObjectIdDictionary
{
    public interface ICompExKeyMan<ExKeyT, CompT, L1T, L2T, L2TRaw> where ExKeyT : IExplodedKey<CompT, L1T, L2T>
    {
        // Join and split exploded key from L1 and L2
        ExKeyT Join(L1T top, L2T bot);
        L1T Split(ExKeyT exKey, out L2T bot);

        // Join and split exploded key from L1 and L2Raw.
        ExKeyT Join(L1T top, L2TRaw bot);
        L1T Split(ExKeyT exKey, out L2TRaw bot);

        // Try version of Join
        bool TryJoin(L1T top, L2TRaw rawBot, out ExKeyT exKey);

        // Try version of Join Comp
        bool TryJoinComp(L1T top, L2TRaw rawBot, out CompT cKey, out L2T bot);

        // Create exploded key from composite key.
        ExKeyT Split(CompT cKey);

        // Join and split composite key from L1 and L2.
        CompT JoinComp(L1T top, L2T bot);
        L1T SplitComp(CompT cKey, out L2T bot);

        // Join and split composite key from L1 and L2Raw.
        CompT JoinComp(L1T top, L2TRaw rawBot);
        L1T SplitComp(CompT cKey, out L2TRaw rawBot);
    }
}
