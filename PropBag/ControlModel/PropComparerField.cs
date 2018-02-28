using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag
{
    public class PropComparerField : NotifyPropertyChangedBase, IEquatable<PropComparerField>, IPropComparerField, IDisposable
    {
        Delegate c;
        bool ure;

        [XmlIgnore]
        public Delegate Comparer { get { return c; } set { this.SetIfDifferentDelegate<Delegate>(ref c, value); } }

        [XmlAttribute("use-reference-equality")]
        public bool UseRefEquality { get { return ure; } set { SetIfDifferent<bool>(ref ure, value); } }

        public PropComparerField() : this(null, false) { }

        public PropComparerField(Delegate comparer, bool useRefEquality)
        {
            Comparer = comparer;
            UseRefEquality = useRefEquality;
        }

        public bool Equals(PropComparerField other)
        {
            if (other == null) return false;

            if (other.Comparer == Comparer && other.UseRefEquality == UseRefEquality) return true;

            return false;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    Comparer = null;
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

        #endregion

    }
}
