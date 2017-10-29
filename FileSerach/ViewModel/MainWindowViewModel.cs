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
using Repository.Implement;
using Repository.Interface;

namespace FileSerach.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Private
        private IRepository _repository;
        private IEnumerable<SearchHistory> _searchHistorys;

        private string _keyWord;
        private IEnumerable<FileResult> _allFiles;
        private IEnumerable<FileResult> _findFiles;
        private FileResult _selectedItem;
        private bool _showIcon = true;
        private bool _showHiddenFile = false;
        private bool _showSysFile = false;

        private Task _loadAllTask;
        #endregion
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

        public bool ShowIcon
        {
            get { return this._showIcon; }
            set
            {
                this._showIcon = value;
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(ShowIcon)));
            }
        }

        public bool ShowHiddenFile
        {
            get { return this._showHiddenFile; }
            set
            {
                this._showHiddenFile = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowHiddenFile)));
            }
        }

        public bool ShowSysFile
        {
            get { return this._showSysFile; }
            set
            {
                this._showSysFile = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowSysFile)));
            }
        }

        public IEnumerable<SearchHistory> SearchHistorys
        {
            get { return this._searchHistorys; }
            set
            {
                this._searchHistorys = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchHistorys)));
            }
        }

        public MainWindowViewModel()
        {
            this._loadAllTask = LoadAll();
            this._repository = new SQLiteRepository();
            var task = this._repository.FindAllAsync<SearchHistory>("ORDER BY DateTime");
            task.ContinueWith(p =>
            {
                this.SearchHistorys = task.Result;
            }, TaskContinuationOptions.NotOnFaulted);
        }

        #region 加载所有
        private Task LoadAll()
        {
            return Task.Factory.StartNew(() =>
            {
                var results = Engine.GetAllFilesAndDirectories();
                this.AllFiles = results?.Select(p =>
                new FileResult()
                {
                    Icon = null,
                    FileName = p.FileName,
                    FullName = p.FullFileName,
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
                var serach = new SearchHistory() { KeyWord = this.KeyWord, DateTime = DateTime.Now };
                this._repository.AddOrUpdateAsync(serach);
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
