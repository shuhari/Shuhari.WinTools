using System.Windows;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for DirNameDialog.xaml
    /// </summary>
    public partial class DirNameDialog : Window
    {
        public DirNameDialog()
        {
            InitializeComponent();

            this.Loaded += DirNameDialog_Loaded;
        }

        public string DirName
        {
            get { return txtDir.Text.Trim(); }
            set { txtDir.Text = value; }
        }

        private void DirNameDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtDir.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDir.Text))
            {
                DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
