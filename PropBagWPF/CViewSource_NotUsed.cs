using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DRM.PropBagWPF
{
    public class CViewSource : CollectionViewSource
    {
        public CViewSource()
        {
        }


        protected override void OnCollectionViewTypeChanged(Type oldCollectionViewType, Type newCollectionViewType)
        {
            base.OnCollectionViewTypeChanged(oldCollectionViewType, newCollectionViewType);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        protected override void OnSourceChanged(object oldSource, object newSource)
        {
            base.OnSourceChanged(oldSource, newSource);
        }

        protected override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return base.ReceiveWeakEvent(managerType, sender, e);
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            return base.ShouldSerializeProperty(dp);
        }
    }
}
