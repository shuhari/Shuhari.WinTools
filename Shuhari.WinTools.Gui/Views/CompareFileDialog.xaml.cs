using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Shuhari.WinTools.Core.Features.ImageFinder;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for CompareFileDialog.xaml
    /// </summary>
    public partial class CompareFileDialog : Window
    {
        public CompareFileDialog()
        {
            InitializeComponent();
        }

        public void SetDirectories(string[] dirs)
        {
            List<DirectoryData> list = new List<DirectoryData>();
            foreach (string path in dirs)
            {
                DirectoryData directoryData = new DirectoryData()
                {
                    Name = path
                };
                if (Directory.Exists(path))
                    directoryData.FileCount = new DirectoryInfo(path).GetFiles().Length;
                list.Add(directoryData);
            }
            this.dg.ItemsSource = (IEnumerable)list;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
