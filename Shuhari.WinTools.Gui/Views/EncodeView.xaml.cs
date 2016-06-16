using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Shuhari.WinTools.Core.Features.Encode.Providers;
using EncodeFeature = Shuhari.WinTools.Core.Features.Encode.Feature;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for EncodeView.xaml
    /// </summary>
    public partial class EncodeView : FeatureView
    {
        public EncodeView()
        {
            InitializeComponent();
        }

        public override void OnCreated()
        {
            var feature = (EncodeFeature)this.Feature;
            var providers = feature.GetProviders();
            cboProvider.ItemsSource = providers;
            cboProvider.SelectedIndex = 0;

            var encodings = Encoding.GetEncodings();
            cboEncoding.ItemsSource = encodings;
            cboEncoding.SelectedItem = encodings.FirstOrDefault(e => e.GetEncoding() == Encoding.UTF8);
        }

        private void cboProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var provider = cboProvider.SelectedItem as EncodeProvider;
            if (provider != null)
            {
                var func = provider.Metadata.Functions;
                rbEncode.IsEnabled = func.HasFlag(EncodeProviderFunctions.Encode);
                rbDecode.IsEnabled = func.HasFlag(EncodeProviderFunctions.Decode);

                if (func == EncodeProviderFunctions.Encode)
                    rbEncode.IsChecked = true;
                else if (func == EncodeProviderFunctions.Decode)
                    rbDecode.IsChecked = true;
            }

            UpdateResult();
        }

        private void rbEncode_Click(object sender, RoutedEventArgs e)
        {
            UpdateResult();
        }

        private void rbDecode_Click(object sender, RoutedEventArgs e)
        {
            UpdateResult();
        }

        private void cboEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateResult();
        }

        private void txtSrc_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateResult();
        }


        private void UpdateResult()
        {
            string result = "";
            var provider = cboProvider.SelectedItem as EncodeProvider;
            var encodingInfo = cboEncoding.SelectedItem as EncodingInfo;
            string src = txtSrc.Text.Trim();
            if (provider != null && encodingInfo != null)
            {
                try
                {
                    if (rbEncode.IsChecked == true)
                        result = provider.Encode(src, encodingInfo.GetEncoding());
                    else if (rbDecode.IsChecked == true)
                        result = provider.Decode(src, encodingInfo.GetEncoding());
                }
                catch(Exception exp)
                {
                    result = "转换异常: " + exp.Message;
                }
            }

            txtResult.Text = result;
        }
    }
}
