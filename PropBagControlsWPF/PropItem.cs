using DRM.TypeSafePropertyBag;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DRM.PropBag.ControlsWPF
{
    public class PropItem : ItemsControl
    {
        static PropItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropItem), new FrameworkPropertyMetadata(typeof(PropItem)));
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(PropItem));

        public string PropertyName
        {
            get
            {
                return (string)this.GetValue(PropertyNameProperty);
            }
            set
            {
                this.SetValue(PropertyNameProperty, value);
            }
        }

        public static readonly DependencyProperty PropKindProperty =
            DependencyProperty.Register("PropKind", typeof(PropKindEnum), typeof(PropBagTemplate),
                new PropertyMetadata(PropKindEnum.Prop));

        public PropKindEnum PropKind
        {
            get
            {
                return (PropKindEnum)this.GetValue(PropKindProperty);
            }
            set
            {
                this.SetValue(PropKindProperty, value);
            }
        }

        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(Type), typeof(PropItem));

        public Type PropertyType
        {
            get
            {
                return (Type)this.GetValue(PropertyTypeProperty);
            }
            set
            {
                this.SetValue(PropertyTypeProperty, value);
            }
        }

        static DependencyProperty HasStoreProperty =
            DependencyProperty.Register("HasStore", typeof(bool), typeof(PropItem), new PropertyMetadata(true));

        public bool HasStore
        {
            get
            {
                return (bool)this.GetValue(HasStoreProperty);
            }
            set
            {
                this.SetValue(HasStoreProperty, value);
            }
        }

        static DependencyProperty TypeIsSolidProperty =
            DependencyProperty.Register("TypeIsSolid", typeof(bool), typeof(PropItem), new PropertyMetadata(true));

        public bool TypeIsSolid
        {
            get
            {
                return (bool)this.GetValue(TypeIsSolidProperty);
            }
            set
            {
                this.SetValue(TypeIsSolidProperty, value);
            }
        }

        public static readonly DependencyProperty ExtraInfoProperty =
            DependencyProperty.Register("ExtraInfo", typeof(string), typeof(PropItem), new PropertyMetadata(null));

        public string ExtraInfo
        {
            get
            {
                return (string)this.GetValue(ExtraInfoProperty);
            }
            set
            {
                this.SetValue(ExtraInfoProperty, value);
            }
        }

        #region Child Management

        override protected void OnVisualChildrenChanged(DependencyObject added, DependencyObject removed)
        {
            if (!DoAllChildrenHaveTheCorrectType(this.Items, out int index, out bool tooMany))
            {
                string elementType = ((Control)this.Items[index]).ToString();
                string elementName = ((Control)this.Items[index]).Name;

                if (tooMany)
                {
                    throw new InvalidOperationException(string.Format("{0} with name = {1} cannot be added here, an element of this type already exists.", elementType, elementName));
                }
                else
                {
                    throw new InvalidOperationException(string.Format("{0} with name = {1} cannot be added here.", elementType, elementName));
                }

            }

            base.OnVisualChildrenChanged(added, removed);
        }

        // TODO: Consider using DependencyProperties on the PropItem UserControl instead of user controls for these complex properties.
        private bool DoAllChildrenHaveTheCorrectType(ItemCollection chils, out int index, out bool tooMany)
        {
            index = -1;
            tooMany = false;

            if (chils == null) return true;

            int initValueFieldCount = 0;
            int doWhenChangedFieldCount = 0;
            int comparerFieldCount = 0;
            int typeInfoFieldCount = 0;

            for (int i = 0; i < chils.Count; i++)
            {
                object item = chils[i];

                if (item is InitialValueField)
                {
                    if (++initValueFieldCount < 2) continue;
                    tooMany = true;
                    index = i;
                    return false;
                }

                if (item is PropDoWhenChangedField)
                {
                    if (++doWhenChangedFieldCount < 2) continue;
                    tooMany = true;
                    index = i;
                    return false;
                }

                if (item is PropComparerField)
                {
                    if (++comparerFieldCount < 2) continue;
                    tooMany = true;
                    index = i;
                    return false;
                }

                if (item is PropTypeInfoField)
                {
                    if (++typeInfoFieldCount < 2) continue;
                    tooMany = true;
                    index = i;
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
