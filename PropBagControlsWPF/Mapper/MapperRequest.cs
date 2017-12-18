using System;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class MapperRequest : Control
    {
        static MapperRequest()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapperRequest),
                new FrameworkPropertyMetadata(typeof(MapperRequest)));
        }

        public MapperRequest()
        {
        }

        public static readonly DependencyProperty SourceTypeProperty =
            DependencyProperty.Register("SourceType", typeof(Type), typeof(MapperRequest));

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
            DependencyProperty.Register("DestinationPropModelKey", typeof(string), typeof(MapperRequest));

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
            DependencyProperty.Register("ConfigPackageName", typeof(string), typeof(MapperRequest));

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
