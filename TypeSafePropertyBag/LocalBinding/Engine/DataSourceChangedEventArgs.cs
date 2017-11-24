using System;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class DataSourceChangedEventArgs : PCGenEventArgs
    {
        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, string propertyName, Type propertyType, object newValue) : base(propertyName, propertyType, newValue)
        {
        }

        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, string propertyName, Type propertyType, object oldValue, object newValue) : base(propertyName, propertyType, oldValue, newValue)
        {
        }

        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, string propertyName, Type propertyType, object oldValue, bool newValueIsUndefined) : base(propertyName, propertyType, oldValue, newValueIsUndefined)
        {
        }

        public DataSourceChangedEventArgs(DataSourceChangeTypeEnum changeType, string propertyName, Type propertyType, bool oldValueIsUndefined, bool newValueIsUndefined) : base(propertyName, propertyType, oldValueIsUndefined, newValueIsUndefined)
        {
        }

        /// <summary>
        /// Reports how the DataSource was changed.
        /// </summary>
        public DataSourceChangeTypeEnum ChangeType { get; private set; }

        static public DataSourceChangedEventArgs NewFromPCGen(PCGenEventArgs eventArgs)
        {
            DataSourceChangedEventArgs result = new DataSourceChangedEventArgs
                (
                DataSourceChangeTypeEnum.PropertyChanged,
                eventArgs.PropertyName,
                eventArgs.PropertyType,
                eventArgs.OldValueIsUndefined,
                eventArgs.NewValueIsUndefined
                );

            if (!eventArgs.OldValueIsUndefined)
            {
                result.OldValue = eventArgs.OldValue;
            }

            if (!eventArgs.NewValueIsUndefined)
            {
                result.NewValue = eventArgs.NewValue;
            }

            return result;
        }

        static public DataSourceChangedEventArgs NewFromPSNodeParentChanged(PSNodeParentChangedEventArgs eventArgs, Type propertyType)
        {
            DataSourceChangedEventArgs result = new DataSourceChangedEventArgs
                (
                DataSourceChangeTypeEnum.ParentHasChanged,
                null,
                propertyType,
                eventArgs.OldValue,
                eventArgs.NewValue
                );

            return result;
        }

    }

}
