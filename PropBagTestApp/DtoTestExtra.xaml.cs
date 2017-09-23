using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.LiveClassGenerator;

using PropBagTestApp.Models;

using AutoMapper;
using System.Collections.Generic;
using System.Reflection;
using DRM.PropBag;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTestExtra : Window
    {

        [PropBagInstanceAttribute("OurData", "There is only one ViewModel in this View.")]
        public DtoTestViewModelExtra OurData
        {
            get
            {
                return (DtoTestViewModelExtra)this.DataContext;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public DtoTestExtra()
        {
            InitializeComponent();

            Grid topGrid = (Grid)this.FindName("TopGrid");

            ViewModelGenerator.StandUpViewModels(topGrid, this);

            // This is an example of how to create the binding whose source is a property in the Property Bag
            // to a UI Element in this class' view.
            // This same binding could have been created in XAML.

            Binding c = new Binding("[System.Double, Size]")
            {
                Converter = new PropValueConverter(),
                ConverterParameter = new TwoTypes(typeof(string), typeof(double)),
                TargetNullValue = string.Empty
            };
            TextBox tb = (TextBox)this.FindName("Sz");
            tb.SetBinding(TextBox.TextProperty, c);
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            MyModel mm = new MyModel
            {
                ProductId = Guid.NewGuid(),
                Amount = 145,
                Size = 17.8
            };

            ReadWithMap(mm, OurData);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            MyModel m1 = SaveWithMap(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelExtra vm)
        {
            IPropBag ip = (IPropBag)vm;
            IEnumerable<MemberInfo> extraMembers = ip.BuildPropertyInfoList<DtoTestViewModelExtra>();

            GetMapper(vm).Map<MyModel, DtoTestViewModelExtra>(mm, vm);
        }

        private MyModel SaveWithMap(DtoTestViewModelExtra vm)
        {
            return GetMapper(vm).Map<DtoTestViewModelExtra, MyModel>(vm);
        }

        private IMapper _mapper = null;
        private IMapper GetMapper(DtoTestViewModelExtra vm)
        {
            if (_mapper == null)
            {
                IPropBag ip = (IPropBag)vm;
                IEnumerable<MemberInfo> extraMembers = ip.BuildPropertyInfoList<DtoTestViewModelExtra>();

                _mapper = new MapperConfiguration(cfg => {
                    cfg
                    .CreateMap<MyModel, DtoTestViewModelExtra>()
                    //.AddExtraDestintionMembers(extraMembers)
                    ;

                    cfg
                    .CreateMap<DtoTestViewModelExtra, MyModel>()
                    //.AddExtraSourceMembers(extraMembers)
                    ;
                }).CreateMapper();
            }
            return _mapper;
        }

    }
}
