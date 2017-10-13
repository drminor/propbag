using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            PropBagTestApp.Properties.Settings.Default.MapperConfigurationProvider = new DRM.PropBag.AutoMapperSupport.MapperConfigurationProvider();
            PropBagTestApp.Properties.Settings.Default.ModuleBuilderInfoProvider = new DRM.PropBag.ViewModelBuilder.DefaultModuleBuilderInfoProvider();

            base.OnStartup(e);
        }
    }
}
