using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ViewModelBuilder
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
