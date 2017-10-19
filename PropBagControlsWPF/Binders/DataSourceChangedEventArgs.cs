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
        public bool DataWasUpdated { get; private set; }

        public string PropertyName { get; private set; }
        public CollectionChangeAction Action { get; private set; }
        public object Element { get; private set; }

        private DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, bool dataWasChanged, string propertyName, 
            CollectionChangeAction action, object element)
        {
            ChangeType = changeType;
            DataWasUpdated = dataWasChanged;
            PropertyName = propertyName;
            Action = action;
            Element = element;
        }

        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, bool dataWasChanged)
            : this(changeType, dataWasChanged, null, CollectionChangeAction.Refresh, null) { }

        public DataSourceChangedEventArgs(string propertyName)
            : this(DataSourceChangeTypeEnum.PropertyChanged, true, propertyName, CollectionChangeAction.Refresh, null) { }

        public DataSourceChangedEventArgs(CollectionChangeAction action, object element)
            : this(DataSourceChangeTypeEnum.CollectionChanged, true, null, action, element) { }

    }

}
