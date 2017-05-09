// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace DotNetEditor.Converters
{
    class InputGestureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new ArgumentException("Can only convert to string.");
            }

            string s = ((KeyGesture) value).DisplayString;
            if (String.IsNullOrEmpty(s))
            {
                s = new KeyGestureConverter().ConvertToString(value);
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException("Conversion is one-way.");
        }
    }

    class CommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RoutedCommand cmd = (RoutedCommand)value;
            RoutedUICommand uiCmd = cmd as RoutedUICommand;
            string s = uiCmd != null ? uiCmd.Text : cmd.Name;
            if (cmd.InputGestures.Count > 0)
            {
                s += " (" + new InputGestureConverter().Convert(cmd.InputGestures[0],
                    typeof(string), null, System.Globalization.CultureInfo.CurrentUICulture) + ")";
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Conversion is one-way.");
        }
    }
}
