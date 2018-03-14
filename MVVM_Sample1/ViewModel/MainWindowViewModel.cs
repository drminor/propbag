using DRM.PropBag;
using DRM.PropBagWPF;
using System;
using System.Threading;

namespace MVVM_Sample1.ViewModel
{
    public partial class MainWindowViewModel : PropBag
    {
        public event EventHandler<EventArgs> RequestClose;

        public void CloseTheWindow()
        {
            Interlocked.CompareExchange(ref RequestClose, null, null)?.Invoke(this, EventArgs.Empty);
        }

        public RelayCommand Close => new RelayCommand(CloseIt);

        private void CloseIt(object o)
        {
            CloseTheWindow();
        }
    }
}
