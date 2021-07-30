// Copyright 2017 Leung Wing-chung. All rights reserved.
// Use of this source code is governed by a GPLv3 license that can be found in
// the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DotNetEditor
{
    public static class AppCommands
    {
        public static readonly RoutedUICommand Run = new RoutedUICommand(
            "Run", "Run", typeof(MainWindow),
            new InputGestureCollection{new KeyGesture(Key.F5)});

        public static readonly RoutedUICommand Stop = new RoutedUICommand(
            "Stop", "Stop", typeof(MainWindow),
            new InputGestureCollection { new KeyGesture(Key.F5, ModifierKeys.Shift) });

        public static readonly RoutedUICommand About = new RoutedUICommand(
            "About", "About", typeof(MainWindow));
    }
}
