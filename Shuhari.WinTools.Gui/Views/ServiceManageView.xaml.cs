using System;
using System.Collections.ObjectModel;
using System.Management;
using System.ServiceProcess;
using System.Windows;
using Shuhari.WinTools.Core.Features.ServiceManage;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for ServiceManageView.xaml
    /// </summary>
    public partial class ServiceManageView : FeatureView
    {
        public ServiceManageView()
        {
            InitializeComponent();

            Loaded += ServiceManageView_Loaded;
        }

        private ObservableCollection<ServiceItem> _services;

        private void ServiceManageView_Loaded(object sender, RoutedEventArgs e)
        {
            _services = new ObservableCollection<ServiceItem>();
            list.ItemsSource = _services;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadServices();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ReloadServices();
        }

        private void ReloadServices()
        {
            _services.Clear();

            var services = ServiceController.GetServices();
            foreach (var service in services)
            {
                var si = new ServiceItem();
                si.Name = service.ServiceName;
                si.DisplayName = service.DisplayName;
                si.Status = service.Status;
                string objPath = string.Format("Win32_Service.Name='{0}'", si.Name);
                using (ManagementObject mo = new ManagementObject(new ManagementPath(objPath)))
                {
                    si.Description = Convert.ToString(mo["Description"]);
                    var sm = (string)mo["StartMode"];
                    ServiceStartMode startMode = ServiceStartMode.Automatic;
                    Enum.TryParse<ServiceStartMode>(sm, out startMode);
                    si.StartMode = startMode;
                }

                _services.Add(si);
                service.Dispose();
            }
        }
    }
}
