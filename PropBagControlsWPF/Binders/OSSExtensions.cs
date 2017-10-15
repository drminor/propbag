using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlsWPF.Binders
{
    public static class OSSExtensions
    {
        public static bool IsReadyOrWatching(this ObservableSourceStatusEnum status)
        {
            return status == ObservableSourceStatusEnum.Ready
                || status == ObservableSourceStatusEnum.IsWatchingColl
                || status == ObservableSourceStatusEnum.IsWatchingProp
                || status == ObservableSourceStatusEnum.IsWatchingPropAndColl;
        }

        public static bool IsWatching(this ObservableSourceStatusEnum status)
        {
            return status == ObservableSourceStatusEnum.IsWatchingColl
                || status == ObservableSourceStatusEnum.IsWatchingProp
                || status == ObservableSourceStatusEnum.IsWatchingPropAndColl;
        }

        public static bool IsWatchingProp(this ObservableSourceStatusEnum status)
        {
            return status == ObservableSourceStatusEnum.IsWatchingProp
                || status == ObservableSourceStatusEnum.IsWatchingPropAndColl;
        }

        public static bool IsWatchingColl(this ObservableSourceStatusEnum status)
        {
            return status == ObservableSourceStatusEnum.IsWatchingColl
                || status == ObservableSourceStatusEnum.IsWatchingPropAndColl;
        }

        public static ObservableSourceStatusEnum SetReady(this ObservableSourceStatusEnum status, bool haveData)
        {
            return (haveData) ? ObservableSourceStatusEnum.Ready : ObservableSourceStatusEnum.NoType;
        }

        public static ObservableSourceStatusEnum SetWatchingColl(this ObservableSourceStatusEnum status)
        {
            return (status == ObservableSourceStatusEnum.IsWatchingProp) ?
                ObservableSourceStatusEnum.IsWatchingPropAndColl : ObservableSourceStatusEnum.IsWatchingColl;
        }

        public static bool NoLongerReady(this ObservableSourceStatusEnum status, ObservableSourceStatusEnum oldStatus)
        {
            return !status.IsReadyOrWatching() && oldStatus.IsReadyOrWatching();
        }

        public static bool IsNowReady(this ObservableSourceStatusEnum status, ObservableSourceStatusEnum oldStatus)
        {
            return status.IsReadyOrWatching() && !oldStatus.IsReadyOrWatching();
        }
    }
}
