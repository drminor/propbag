using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropBagLib
{
    /// <summary>
    /// Notifies clients that a property value has changed and provides the old and new values.
    /// </summary>
    public interface INotifyPropertyChangedWithVals
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        event PropertyChangedWithValsHandler PropertyChangedWithVals;
    }
}

