using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using FileExplorer.ViewModel;

namespace FileExplorer.View
{
    /// <summary>
    /// MainContentControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainContentControl:UserControl
    {
        public MainContentControl()
        {
            InitializeComponent();
            DataContext=new MainViewModel();
        }

        private void DirectoryTree_SelectedItemChanged(object sender,RoutedPropertyChangedEventArgs<object> e)
        {
            string selectedPath = e.NewValue as string;
            if(selectedPath!=null)
            {
                // 调用ViewModel中的方法来加载目录内容
                ((MainViewModel)DataContext).LoadDirectoryContentAsync(selectedPath);
            }
        }


        private void FileListView_MouseDoubleClick(object sender,MouseButtonEventArgs e)
        {
            var selectedItem = FileListView.SelectedItem as FileSystemInfo;
            if(selectedItem!=null&&selectedItem is DirectoryInfo)
            {
                // 处理目录双击事件
                ((MainViewModel)DataContext).NavigateToDirectory(selectedItem.FullName);
            }
        }

        private void FileListView_MouseLeftButtonUp(object sender,MouseButtonEventArgs e)
        {
            var selectedItem = FileListView.SelectedItem as FileSystemInfo;
            if(selectedItem!=null&&selectedItem is FileInfo)
            {
                // 处理文件单击事件
                ((MainViewModel)DataContext).SelectFile(selectedItem);
            }
        }
    }
}
