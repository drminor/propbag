using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    public static class ObjectIdIssuer
    {
        private static int m_Counter = 0;

        public static int NextObjectId
        {
            get
            {
                return System.Threading.Interlocked.Increment(ref m_Counter);
            }
        }
    }
}
