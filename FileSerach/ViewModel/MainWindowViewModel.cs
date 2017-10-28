using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileSerach.Command;
using FileSerach.Model;
using QueryEngine;

namespace FileSerach.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _keyWord;
        private IEnumerable<FileResult> _allFiles;
        private IEnumerable<FileResult> _findFiles;
        private FileResult _selectedItem;

        private Task _loadAllTask;
        public event PropertyChangedEventHandler PropertyChanged;

        public string KeyWord
        {
            get { return this._keyWord; }
            set
            {
                this._keyWord = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.KeyWord)));
            }
        }

        public IEnumerable<FileResult> AllFiles
        {
            get { return this._allFiles; }
            set
            {
                this._allFiles = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.AllFiles)));
            }
        }

        public IEnumerable<FileResult> FindFiles
        {
            get { return this._findFiles; }
            set
            {
                this._findFiles = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FindFiles)));
            }
        }

        public FileResult SelectedItem
        {
            get { return this._selectedItem; }
            set
            {
                this._selectedItem = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedItem)));
            }
        }

        public MainWindowViewModel()
        {
            //this.AllFiles = new List<FileResult>()
            //{
            //    new FileResult(){ Icon = null, FileName = "Test1.txt", FullName = @"C:\Test1.txt", CreateDateTime = DateTime.Now, Size = "1KB"},
            //    new FileResult(){Icon = null, FileName = "TestSame.txt", FullName = @"C:\TestSame.txt", Size = "2KB", CreateDateTime = DateTime.Now}
            //};

            this._loadAllTask = LoadAll();
        }

        #region 加载所有
        private Task LoadAll()
        {
            return Task.Factory.StartNew(() =>
            {
                var results = Engine.GetAllFilesAndDirectories();
                this.AllFiles = results?.Select(p => new FileResult()
                {
                    Icon = null,
                    FileName = p.FileName,
                    FullName = p.FullFileName,
                    Size = "1KB",
                    CreateDateTime = DateTime.Now
                });
            });
        }
        #endregion

        #region 查找命令
        private Task Find()
        {
            Action<object> search = o =>
            {
                if (string.IsNullOrWhiteSpace(this.KeyWord))
                    this.FindFiles = new List<FileResult>();
                this.FindFiles = this.AllFiles.Where(p => p.FileName.Contains(this.KeyWord));
            };

            if (this._loadAllTask.IsCompleted)
                return Task.Factory.StartNew(search, null);
            else
                return this._loadAllTask.ContinueWith(search);
        }

        private bool CanFind()
        {
            return true;
        }

        public ICommand FindCommand
        {
            get { return new RelayCommand(Find, CanFind); }
        }
        #endregion

        #region 打开文件
        private Task Open()
        {
            if (this.SelectedItem == null)
                return Task.Delay(0);
            var fileName = this.SelectedItem.FullName;
            return Task.Factory.StartNew(() => Process.Start(fileName));
        }

        private bool CanOpen()
        {
            if (this.SelectedItem == null)
                return false;
            var fileName = this.SelectedItem.FullName;
            return File.Exists(fileName);
        }

        public ICommand OpenCommand
        {
            get { return new RelayCommand(Open, CanOpen); }
        }
        #endregion

        #region 定位文件
        private Task Location()
        {
            if (this.SelectedItem == null)
                return Task.Delay(0);
            var fileName = this.SelectedItem.FullName;
            return Task.Factory.StartNew(() => Process.Start("explorer.exe", "/select, " + fileName));
        }

        private bool CanLocation()
        {
            if (this.SelectedItem == null)
                return false;
            return File.Exists(this.SelectedItem.FullName);
        }

        public ICommand LocationCommand
        {
            get { return new RelayCommand(Location, CanLocation); }
        }
        #endregion
    }
}
