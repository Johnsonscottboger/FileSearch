using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using log4net;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Config/log4net.config", Watch = true)]
namespace FileSerach
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static ILog log;

        public App()
        {
            log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                log.Error(args.ExceptionObject);
                MessageBox.Show($"Unhandled exception.{Environment.NewLine}{args.ExceptionObject.ToString()}");
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                //MessageBox.Show(args.Exception.Message);
                log.Error(args.Exception);
                args.SetObserved();
            };
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            log.Error(e.Exception);
            MessageBox.Show("Error encountered! Please contact support." + Environment.NewLine + e.Exception.Message);
            e.Handled = true;
        }
    }
}
