using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.ComponentModel.Design.Serialization;

using DRM.PropBag.Caches;
using DRM.PropBag.ControlModel;

using System.Xaml;
using System.Windows.Markup;
using System.Windows.Data;

namespace DRM.PropBag.ControlsWPF
{
    [MarkupExtensionReturnType(typeof(object))]
    public class TwoTypesExtension : MarkupExtension
    {
        public Type SourceType { get; set; }
        public Type DestType { get; set; }

        /// <summary>
        /// Used when the source type is System.String
        /// </summary>
        /// <param name="destinationType"></param>
        public TwoTypesExtension(Type destinationType) : this(typeof(string), destinationType) { }

        public TwoTypesExtension(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestType = destinationType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (SourceType == null || DestType == null)
                throw new ArgumentException("Type argument is not specified");

            return new ControlModel.TwoTypes(SourceType, DestType);
        }

    }
}
