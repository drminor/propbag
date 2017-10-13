using System;
using System.ComponentModel;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public class DataSourceChangedEventArgs : EventArgs
    {

        /// <summary>
        /// Reports how the DataSource was changed.
        /// Currently this is always set to refresh.
        /// </summary>
        public DataSourceChangeTypeEnum ChangeType { get; private set; }
        public string PropertyName { get; private set; }
        public CollectionChangeAction Action { get; private set; }
        public object Element { get; private set; }

        private DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, string propertyName, 
            CollectionChangeAction action, object element)
        {
            ChangeType = changeType;
            PropertyName = propertyName;
            Action = action;
            Element = element;
        }

        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType)
            : this(changeType, null, CollectionChangeAction.Refresh, null) { }

        public DataSourceChangedEventArgs(string propertyName)
            : this(DataSourceChangeTypeEnum.PropertyChanged, propertyName, CollectionChangeAction.Refresh, null) { }

        public DataSourceChangedEventArgs(CollectionChangeAction action, object element)
            : this(DataSourceChangeTypeEnum.CollectionChanged, null, action, element) { }

    }

}
