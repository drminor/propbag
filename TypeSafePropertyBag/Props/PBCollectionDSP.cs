using System;
using System.Collections;
using System.Windows.Data;

namespace DRM.TypeSafePropertyBag
{
    // Takes a IProp<CT,T> where CT: is an ObservableCollection<T> and where CT raises the ItemEndEdit event.
    // It calls for a refresh, which results in the binder to call our BeginQuery method,
    // which will raise our DataChanged event, if the data produced by BeginQuery is different from the 'current data'.

    internal class PBCollectionDSP : DataSourceProvider, INotifyItemEndEdit
    {
        #region Private Properties

        IWatchAPropItemGen _propItemWatcherGen;

        #endregion

        public event EventHandler<EventArgs> ItemEndEdit;

        #region Constructor

        public PBCollectionDSP(IWatchAPropItemGen propItemWatcher/*, bool isAsynchronous*/)
        {
            _propItemWatcherGen = propItemWatcher;
        }

        #endregion

        #region Public Properties

        public bool IsAsynchronous => false;

        #endregion

        #region DataSourceProvider Overrides

        protected override void BeginQuery()
        {
            try
            {
                // Data holds a reference the previously fetched data, if any.
                if (Data is INotifyItemEndEdit inieeCurrent)
                {
                    inieeCurrent.ItemEndEdit -= Iniee_ItemEndEdit;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Could not remove ItemEndEdit handler. The exception description is {e.Message}.");
            }

            IList data = _propItemWatcherGen.GetValue() as IList;

            if (data is INotifyItemEndEdit inieeNew)
            {
                inieeNew.ItemEndEdit += Iniee_ItemEndEdit;
            }

            OnQueryFinished(data);
        }

        #endregion

        #region Event Handlers

        private void Iniee_ItemEndEdit(object sender, EventArgs e)
        {
            OnItemEndEdit(sender, e);
        }

        private void DoWhenListSourceIsReset(object sender, PcGenEventArgs args)
        {
            if (!IsRefreshDeferred)
            {
                Refresh();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not refreshing, we are in a Deferred Refresh Cycle.");
            }
        }

        #endregion

        #region Raise Event Helpers

        protected void OnItemEndEdit(object sender, EventArgs e)
        {
            ItemEndEdit?.Invoke(sender, e);
        }

        #endregion
    }
}
