using System;
using System.Windows;
using System.Windows.Threading;
using Shuhari.Library.Common.Utils;

namespace Shuhari.WinTools.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                e.Dispatcher.Invoke(() => ReportException(e.Exception));
                e.Handled = true;
            }
        }

        private void ReportException(Exception exp)
        {
            MessageBox.Show(exp.GetFullTrace());
            LogException(exp);
        }

        internal static void LogException(Exception exp)
        {
            exp.LogToFile(@"{base}\error.log");
        }
    }
}
