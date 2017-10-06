using PropBagTestApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropBagTestApp.ViewModels
{
    public class ReferenceBindViewModel : INotifyPropertyChanged
    {

        public ReferenceBindViewModel()
        {
            System.Diagnostics.Debug.WriteLine("ReferenceBindViewModel is being created.");
            Deep = new MyModel4();
        }

        Guid _productId;
        public Guid ProductId
        {
            get { return _productId; }
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    OnPropertyChanged(nameof(ProductId));
                }
            }
        }

        int _amount;
        public int Amount
        {
            get { return _amount; }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        double _size;
        public double Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        double _testDouble;
        public double TestDouble
        {
            get { return _testDouble; }
            set
            {
                if (_testDouble != value)
                {
                    _testDouble = value;
                    OnPropertyChanged(nameof(TestDouble));
                }
            }
        }

        MyModel4 _deep;
        public MyModel4 Deep
        {
            get { return _deep; }
            set
            {
                if (_deep != value)
                {
                    _deep = value;
                    OnPropertyChanged(nameof(Deep));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
