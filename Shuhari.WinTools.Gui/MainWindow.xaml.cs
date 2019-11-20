using System;
using System.Windows;
using System.Windows.Controls;
using Shuhari.WinTools.Core.Features;
using Shuhari.WinTools.Gui.Services;
using Shuhari.WinTools.Gui.Views;

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
            System.Console.WriteLine("Hello");
        }

        private BaseFeature[] _features;
        private ApplicationService _service;

        /// <summary>
        /// print hello world
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _service = new ApplicationService();
            _features = _service.LoadFeatures(featureStack);
            featureList.ItemsSource = _features;
            featureStack.ActiveChild = null;
        }

        private void featureList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fm = featureList.SelectedItem as BaseFeature;
            if (fm != null)
            {
                var view = _service.EnsureView(fm, featureStack);
                if (view != featureStack.ActiveChild)
                {
                    featureStack.ActiveChild = view;
                    (featureStack.ActiveChild as FeatureView)?.OnDeactivated();
                    view?.OnActivated();
                }
            }
        }
    }
}
