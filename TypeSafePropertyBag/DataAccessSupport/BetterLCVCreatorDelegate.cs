using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public delegate ICollectionView BetterLCVCreatorDelegate<T>(ObservableCollection<T> list) where T: INotifyItemEndEdit;
}
