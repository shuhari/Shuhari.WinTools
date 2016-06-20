using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Shuhari.Library.Utils;
using Shuhari.Library.Windows;
using Shuhari.WinTools.Core.Features.RenameFile;

namespace Shuhari.WinTools.Gui.Views
{
    /// <summary>
    /// Interaction logic for RenameFileView.xaml
    /// </summary>
    public partial class RenameFileView : FeatureView
    {
        public RenameFileView()
        {
            InitializeComponent();

            Loaded += RenameFileView_Loaded;
        }

        private ObservableCollection<RenameItem> _items;

        private void RenameFileView_Loaded(object sender, RoutedEventArgs e)
        {
            var types = new EnumItem[] {
                new EnumItem("包含", 0),
                new EnumItem("前缀", 1),
                new EnumItem("后缀", 2),
                new EnumItem("正则表达式", 3),
            };
            cboType1.ItemsSource = types;
            cboType1.SelectedIndex = 0;
            cboType2.ItemsSource = types;
            cboType2.SelectedIndex = 0;
            cboType3.ItemsSource = types;
            cboType3.SelectedIndex = 0;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = this.BrowseForFolder();
            if (path != null)
                txtDirName.Text = path;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var option = new Option();
                option.BaseDirectory = txtDirName.Text.Trim();
                option.Option1 = txtOption1.Text.Trim();
                option.Type1 = (int)cboType1.SelectedValue;
                option.Replace1 = txtReplace1.Text.Trim();
                option.Option2 = txtOption2.Text.Trim();
                option.Type2 = (int)cboType2.SelectedValue;
                option.Replace2 = txtReplace2.Text.Trim();
                option.Option3 = txtOption3.Text.Trim();
                option.Type3 = (int)cboType3.SelectedValue;
                option.Replace3 = txtReplace3.Text.Trim();
                option.ApplyToDirectory = (bool)chkApplyToFolder.IsChecked;
                option.ApplyToFile = (bool)chkApplyToFile.IsChecked;

                if (option.BaseDirectory.IsBlank())
                    throw new ArgumentException("请输入或选择目录");
                if (option.Option1.IsBlank() &&
                    option.Option2.IsBlank() &&
                    option.Option3.IsBlank())
                    throw new ArgumentException("请输入查找/替换选项");
                if (!option.ApplyToDirectory && !option.ApplyToFile)
                    throw new ArgumentException("请选择目录或文件");

                _items = new ObservableCollection<RenameItem>();
                ProcessDir(new DirectoryInfo(option.BaseDirectory), option);
                resultList.ItemsSource = _items;
            }
            catch (ArgumentException exp)
            {
                MessageBox.Show(exp.Message, "选项错误");
            }
        }

        private void ProcessDir(DirectoryInfo di, Option option)
        {
            foreach (var subDi in di.GetDirectories())
                ProcessDir(subDi, option);

            if (option.ApplyToFile)
            {
                foreach (var fi in di.GetFiles())
                {
                    var newName = option.GetNewName(fi.Name);
                    if (newName != fi.Name)
                    {
                        _items.Add(new RenameItem(fi.Name, newName, fi.DirectoryName));
                    }
                }
            }

            if (option.ApplyToDirectory && di.FullName.Length != 3)
            {
                var newName = option.GetNewName(di.Name);
                if (newName != di.Name)
                    _items.Add(new RenameItem(di.Name, newName, di.Parent.FullName));
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(it => it.Selected))
            {
                var currentPath = Path.Combine(item.DirectoryName, item.CurrentName);
                var newPath = Path.Combine(item.DirectoryName, item.NewName);
                var fi = new FileInfo(currentPath);
                try
                {
                    fi.MoveTo(newPath);
                }
                catch (Exception /*exp*/)
                {
                    // TODO : notify exception
                }
            }
        }
    }

    class EnumItem
    {
        public EnumItem(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; private set; }

        public int Value { get; private set; }
    }


    class Option
    {
        public string BaseDirectory { get; set; }

        public bool ApplyToDirectory { get; set; }

        public bool ApplyToFile { get; set; }

        public string Option1 { get; set; }

        public int Type1 { get; set; }

        public string Replace1 { get; set; }

        public string Option2 { get; set; }

        public int Type2 { get; set; }

        public string Replace2 { get; set; }

        public string Option3 { get; set; }

        public int Type3 { get; set; }

        public string Replace3 { get; set; }

        public string GetNewName(string name)
        {
            var namePart = Path.GetFileNameWithoutExtension(name);
            var extPart = Path.GetExtension(name);

            namePart = Replace(namePart, Option1, Type1, Replace1);
            namePart = Replace(namePart, Option2, Type2, Replace2);
            namePart = Replace(namePart, Option3, Type3, Replace3);
            return string.Format("{0}{1}", namePart, extPart);
        }

        private string Replace(string name, string subStr, int type, string replaceWith)
        {
            if (subStr.IsBlank())
                return name;

            if (type == 0) // Contains
            {
                name = name.Replace(subStr, replaceWith);
            }
            else if (type == 1 && name.StartsWith(subStr)) // Start with
            {
                name = replaceWith + name.Substring(subStr.Length);
            }
            else if (type == 2) // Ends with
            {
                name = name.Substring(0, name.Length - subStr.Length) + replaceWith;
            }
            else if (type == 3)
            {
                var re = new Regex(subStr);
                name = re.Replace(name, replaceWith);
            }

            return name.Trim();
        }
    }
}
