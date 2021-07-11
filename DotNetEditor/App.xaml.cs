using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DotNetEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string AppName = "Dot Net Editor";
        public static Bugsnag.Client client;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Colored output only works if this is an console app. We do not want the console
            // anyway, so hide it.
            ConsoleUtil.HideConsoleWindow();

            if (client == null)
                client = new Bugsnag.Client(Bugsnag.ConfigurationSection.Configuration.Settings);
        }
    }
}
