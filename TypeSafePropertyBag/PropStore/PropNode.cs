using System;


namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    internal class PropNode : IDisposable
    {
        #region Constructor

        public PropNode(PropIdType propId, IPropDataInternal propData_Internal, BagNode parent)
        {
            CompKey = new SimpleExKey(parent.ObjectId, propId);
            PropData_Internal = propData_Internal ?? throw new ArgumentNullException(nameof(propData_Internal));

            //parent.AddChild(this);
            Parent = parent;

            Child = null;
        }

        #endregion

        #region Public Properties

        // This composite key identifies both the IPropBag and the Prop. Its a globally unique PropId.
        public ExKeyT CompKey { get; }

        public ObjectIdType ObjectId => CompKey.Level1Key;
        public PropIdType PropId => CompKey.Level2Key;

        internal IPropDataInternal PropData_Internal { get; }

        public bool HoldsAPropBag => PropData_Internal.IsPropBag;

        public BagNode Parent { get; set; }

        public BagNode Child { get; set; }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Parent = null;
                    Child = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return CompKey.ToString();
        }

        #endregion
    }
}
