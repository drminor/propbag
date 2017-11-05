namespace DRM.TypeSafePropertyBag.Fundamentals.NotUsed
{
    public class GenerateHash
    {
        public static int CustomHash(int seed, int factor, params int[] vals)
        {
            int hash = seed;
            foreach (int i in vals)
            {
                hash = (hash * factor) + i;
            }
            return hash;
        }

        public static int NICE_SEED = 1009;
        public static int NICE_FACTOR = 9176;

        public static int CustomHash(int hash1, int hash2)
        {
            return CustomHash(NICE_SEED, NICE_FACTOR, new int[] { hash1, hash2});
        }

        public static int CustomHash(int hash1, int hash2, int hash3)
        {
            return CustomHash(NICE_SEED, NICE_FACTOR, new int[] { hash1, hash2, hash3 });
        }
    }
}
