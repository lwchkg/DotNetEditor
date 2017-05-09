﻿using System;
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

        public static readonly RoutedUICommand About = new RoutedUICommand(
            "About", "About", typeof(MainWindow));
    }
}
