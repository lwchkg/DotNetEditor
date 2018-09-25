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
            if (client == null)
                client = new Bugsnag.Client(Bugsnag.ConfigurationSection.Configuration.Settings);
        }
    }
}
