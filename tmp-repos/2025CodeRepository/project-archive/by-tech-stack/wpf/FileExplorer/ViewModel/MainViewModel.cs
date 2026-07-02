using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FileExplorer.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Drives { get; set; }
        public ObservableCollection<string> CurrentDirectoryContent { get; set; }

        public Dispatcher Dispatcher { get; set; }

        public MainViewModel()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            Drives = new ObservableCollection<string>();
            CurrentDirectoryContent = new ObservableCollection<string>();
            LoadDrivesAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async Task LoadDrivesAsync()
        {
            await Task.Run(() =>
            {
                var drives = DriveInfo.GetDrives().Select(d => d.Name).ToList();
                foreach (var drive in drives)
                {
                    Drives.Add(drive);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Drives)));
            });
        }

        public async Task LoadDirectoryContentAsync(string path)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() => CurrentDirectoryContent.Clear());
                var directories = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);

                foreach (var dir in directories)
                {
                    Dispatcher.Invoke(() => CurrentDirectoryContent.Add(dir));
                }

                foreach (var file in files)
                {
                    Dispatcher.Invoke(() => CurrentDirectoryContent.Add(file));
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDirectoryContent)));
            });
        }

        public async Task NavigateToDirectory(string path)
        {
            await LoadDirectoryContentAsync(path);
        }

        public void SelectFile(FileSystemInfo fileInfo)
        {
            if (fileInfo is FileInfo file)
            {
                SelectedFilePath = file.FullName;
                SelectedFileSize = file.Length;
                SelectedFileType = file.Extension;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFilePath)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFileSize)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFileType)));
            }
        }

        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                _selectedFilePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFilePath)));
            }
        }

        private long _selectedFileSize;
        public long SelectedFileSize
        {
            get => _selectedFileSize;
            set
            {
                _selectedFileSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFileSize)));
            }
        }

        private string _selectedFileType;
        public string SelectedFileType
        {
            get => _selectedFileType;
            set
            {
                _selectedFileType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFileType)));
            }
        }
    }
}