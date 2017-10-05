using System;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class DataSourceChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Reports how the DataSource was changed.
        /// Currently this is always set to refresh.
        /// </summary>
        public DataSourceChangeTypeEnum ChangeType { get; private set; }

        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType) : base()
        {
            ChangeType = changeType;
        }


    }

}
