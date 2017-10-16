using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ViewModelBuilder;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Properties;
using System;
using System.ComponentModel;
using System.Windows;


namespace PropBagTestApp
{
    internal static class SettingsExtensions
    {
        public static ConfiguredMappers ConfiguredMappers { get; private set; }
        public static IModuleBuilderInfo PropBagProxyBuilder { get; private set; }
        public static PropBagMappingStrategyEnum MappingStrategy { get; private set; }
        public static IPropFactory ThePropFactory { get; }

        static SettingsExtensions()
        {
            GetIsInDesignModeStatic();

            Settings theAppsSettings = Settings.Default;

            // Use the settings to initialize our static properties.
            theAppsSettings.GetSettings();

            // Create a shared Prop Factory
            ThePropFactory = new PropFactory(false, GetTypeFromName);
        }

        public static void GetSettings(this Settings settings)
        {
            if (InDesignMode())
            {
                try
                {
                    MappingStrategy = settings.MappingStrategy;
                    ConfiguredMappers = CachesProvider.GetMapperCache(MappingStrategy);
                    DefaultModuleBuilderInfoProvider defaultPropBagProxyBuilderProvider = settings.ModuleBuilderInfoProvider;
                    PropBagProxyBuilder = defaultPropBagProxyBuilderProvider.ModuleBuilderInfo;
                }
                catch
                {
                    MappingStrategy = PropBagMappingStrategyEnum.EmitProxy;
                    ConfiguredMappers = CachesProvider.GetMapperCache(MappingStrategy);

                    PropBagProxyBuilder = new DefaultModuleBuilderInfoProvider().ModuleBuilderInfo;
                }
            }
            else
            {
                try
                {
                    MappingStrategy = settings.MappingStrategy;
                    ConfiguredMappers = CachesProvider.GetMapperCache(MappingStrategy);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Settings.MappingStrategy has an undefined value.");
                }

                try
                {
                    DefaultModuleBuilderInfoProvider defaultPropBagProxyBuilderProvider = settings.ModuleBuilderInfoProvider;
                    PropBagProxyBuilder = defaultPropBagProxyBuilderProvider.ModuleBuilderInfo;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Settings.ModuleBuilderInfoProvider has an undefined value.");
                }

            }
        }

        public static Type GetTypeFromName(string typeName)
        {
            Type result;
            try
            {
                result = Type.GetType(typeName);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", e);
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }

            return result;
        }

        #region InDesign Support
        public static bool InDesignMode() => _isInDesignMode.HasValue && _isInDesignMode == true;

        public static bool? _isInDesignMode;

        public static bool GetIsInDesignModeStatic()
        {
            if (!_isInDesignMode.HasValue)
            {
                var prop = DesignerProperties.IsInDesignModeProperty;
                _isInDesignMode
                    = (bool)DependencyPropertyDescriptor
                                    .FromProperty(prop, typeof(FrameworkElement))
                                    .Metadata.DefaultValue;
            }

            return _isInDesignMode.Value;
        }
        #endregion

    }
}
