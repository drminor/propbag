using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DRM.PropBag.ControlsWPF
{
    public class PropItem : ItemsControl
    {
        static PropItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropItem), new FrameworkPropertyMetadata(typeof(PropItem)));
        }

        override protected void OnVisualChildrenChanged(DependencyObject added, DependencyObject removed)
        {
            int index;
            bool tooMany;
            if (!DoAllChildrenHaveTheCorrectType(this.Items, out index, out tooMany))
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

        private bool DoAllChildrenHaveTheCorrectType(ItemCollection chils, out int index, out bool tooMany)
        {
            index = -1;
            tooMany = false;

            if (chils == null) return true;

            int initValueFieldCount = 0;
            int doWhenChangedFieldCount = 0;
            //int comparerFieldCount = 0;

            for (int i = 0; i < chils.Count; i++)
            {
                object item = chils[i];

                if (item is InitialValueField)
                {
                    if(++initValueFieldCount < 2) continue;
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
            }

            return true;
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(PropItem), new PropertyMetadata(null));

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

        //public static readonly DependencyProperty InitialValueFieldProperty =
        //    DependencyProperty.Register("InitialValueField", typeof(InitialValueField), typeof(PropItem));

        //public InitialValueField InitialValueField
        //{
        //    get
        //    {
        //        return (InitialValueField)this.GetValue(InitialValueFieldProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(InitialValueFieldProperty, value);
        //    }
        //}

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

        #region Unused

        private bool IsNewVisChildOk(DependencyObject added, out string elName)
        {
            elName = "not applicable";

            if (added == null) return true;
            if (added is ContentPresenter) return true;
            if (added is ContentControl)
            {
                elName = ((ContentControl)added).Name;
                return elName == "InitialValueFieldContent" || elName == "" || elName == null;
            }

            if (added is InitialValueField) return true;

            return false;
        }

        #endregion



    }
}
