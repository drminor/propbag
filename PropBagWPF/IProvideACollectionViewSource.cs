using System.Windows.Data;

namespace DRM.PropBagWPF
{
    public interface IProvideACollectionViewSource
    {
        CollectionViewSource CollectionViewSource { get; }
    }
}
