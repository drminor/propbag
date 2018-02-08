using System;

namespace DRM.TypeSafePropertyBag
{
    internal class UnsubscriberForPropStore : IDisposable
    {
        WeakReference<BagNode> _wr;
        ParentNCSubscriptionRequest _request;

        public UnsubscriberForPropStore(WeakReference<BagNode> wr_us, ParentNCSubscriptionRequest request)
        {
            _wr = wr_us;
            _request = request;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_wr.TryGetTarget(out BagNode bagNode))
                    {
                        bagNode.UnsubscribeToParentNodeHasChanged(_request);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

        }

        #endregion
    }

}
