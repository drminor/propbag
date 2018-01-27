
namespace DRM.PropBag.TypeWrapper
{
    public static class AssemblyIdIssuer
    {
        private static int m_Counter = 0;

        public static int NextModuleId
        {
            get
            {
                return System.Threading.Interlocked.Increment(ref m_Counter);
            }
        }
    }
}
