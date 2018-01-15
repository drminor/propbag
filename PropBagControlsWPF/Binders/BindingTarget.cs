using DRM.PropBagControlsWPF.WPFHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DRM.PropBagControlsWPF.Binders
{
    public class BindingTarget
    {
        public BindingTarget(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            DependencyObject = dependencyObject ?? throw new ArgumentNullException(nameof(dependencyObject));
            DependencyProperty = dependencyProperty ?? throw new ArgumentNullException(nameof(dependencyProperty));
            PropertyInfo = null;
        }

        public BindingTarget(DependencyObject dependencyObject, PropertyInfo propertyInfo)
        {
            DependencyObject = dependencyObject ?? throw new ArgumentNullException(nameof(dependencyObject));
            DependencyProperty = null;
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        }

        public DependencyObject DependencyObject { get; }
        public DependencyProperty DependencyProperty { get; }
        public PropertyInfo PropertyInfo { get; }

        public bool IsDependencyProperty => DependencyProperty != null;
        public bool IsProperty => PropertyInfo != null;

        public bool IsDataContext
        {
            get
            {
                if (DependencyProperty != null)
                {
                    if (DependencyProperty.Name == "DataContext")
                        return true;
                    else
                        return false;
                }
                return false;
            }
        }



        public Type PropertyType
        {
            get
            {
                if(IsDependencyProperty)
                {
                    return DependencyProperty.PropertyType;
                }
                else
                {
                    return PropertyInfo.PropertyType;
                }
            }
        }

        public string ObjectName
        {
            get
            {
                return LogicalTree.GetNameFromDepObject(DependencyObject);
            }
        }

        public string PropertyName
        {
            get
            {
                if (IsDependencyProperty)
                {
                    return DependencyProperty.Name;
                }
                else
                {
                    return PropertyInfo.Name;
                }
            }

        }

    }
}
