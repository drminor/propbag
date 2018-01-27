using System;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBagControlsWPF
{
    public class MapperRequestTemplate : Control
    {
        static MapperRequestTemplate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapperRequestTemplate),
                new FrameworkPropertyMetadata(typeof(MapperRequestTemplate)));
        }

        public static readonly DependencyProperty SourceTypeProperty =
            DependencyProperty.Register("SourceType", typeof(Type), typeof(MapperRequestTemplate));

        public Type SourceType
        {
            get
            {
                return (Type)this.GetValue(SourceTypeProperty);
            }
            set
            {
                this.SetValue(SourceTypeProperty, value);
            }
        }

        public static readonly DependencyProperty DestinationPropModelKeyProperty =
            DependencyProperty.Register("DestinationPropModelKey", typeof(string), typeof(MapperRequestTemplate));

        public string DestinationPropModelKey
        {
            get
            {
                return (string)this.GetValue(DestinationPropModelKeyProperty);
            }
            set
            {
                this.SetValue(DestinationPropModelKeyProperty, value);
            }
        }

        public static readonly DependencyProperty ConfigPackageNameProperty =
            DependencyProperty.Register("ConfigPackageName", typeof(string), typeof(MapperRequestTemplate));

        public string ConfigPackageName
        {
            get
            {
                return (string)this.GetValue(ConfigPackageNameProperty);
            }
            set
            {
                this.SetValue(ConfigPackageNameProperty, value);
            }
        }
    }
}
