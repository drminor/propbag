﻿
using System;
using System.Windows.Data;
using System.Globalization;
using System.Reflection;

/// <remarks>
/// This was copied verbatum from:
/// https://www.codeproject.com/Articles/29495/Binding-and-Using-Friendly-Enums-in-WPF
/// and is licenced using "The Code Project Open License (CPOL)"
/// 
/// TODO: Check on https://github.com/TylerBrinkley/Enums.NET
/// </remarks>
namespace DRM.PropBagControlsWPF.Converters
{
    
    /// <summary>
    /// This class simply takes an enum and uses some reflection to obtain
    /// the friendly name for the enum. Where the friendlier name is
    /// obtained using the LocalizableDescriptionAttribute, which holds the localized
    /// value read from the resource file for the enum
    /// </summary>
    [ValueConversion(typeof(object), typeof(String))]
    public class EnumToFriendlyNameConverter : IValueConverter
    {
        #region IValueConverter implementation

        /// <summary>
        /// Convert value for binding from source object
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // To get around the stupid WPF designer bug
            if (value != null)
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                // To get around the stupid WPF designer bug
                if (fi != null)
                {
                    var attributes =
                        (LocalizableDescriptionAttribute[]) fi.GetCustomAttributes(typeof(LocalizableDescriptionAttribute), false);

                    return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description)))
                                ? attributes[0].Description
                                : value.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// ConvertBack value from binding back to source object
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Cant convert back");
        }

        #endregion
    }
}

