using System.Windows;
using System.Windows.Controls;
using Shuhari.WinTools.Gui.Models;
using Shuhari.WinTools.Gui.Services;

namespace Shuhari.WinTools.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private FeatureModel[] _features;
        private ApplicationService _service;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _service = new ApplicationService();
            _features = _service.CreateModels(featureStack);
            featureList.ItemsSource = _features;
        }

        private void featureList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fm = featureList.SelectedItem as FeatureModel;
            if (fm != null && fm.View != null)
            {
                featureStack.ActiveChild = fm.View;
            }
        }
    }
}
