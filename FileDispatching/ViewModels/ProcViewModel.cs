using Caliburn.Micro;
using FileDispatching.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FileDispatching.ViewModels
{
    public class ProcViewModel : Screen
    {
        private const string BUTTON_NAME_CANCEL = "キャンセル";
        private const string BUTTON_NAME_BACK = "戻る";

        private readonly INavigationService _navigationService;
        private string _cancelButtonName;
        private int _progressMax;
        private int _progressValue;
        private object _selectedProgressItem;
        private FileDispatchManager _fileManager;
        private CancellationTokenSource _cancelTokenSource;
        private Progress<ProgressItem> _progress;

        public string FolderPath
        {
            get;
            set;
        }

        public string RegexPattern
        {
            get;
            set;
        }

        public string CancelButtonName
        {
            get { return _cancelButtonName; }
            set
            {
                _cancelButtonName = value;
                NotifyOfPropertyChange(() => CancelButtonName);
            }
        }

        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                _progressMax = value;
                NotifyOfPropertyChange(() => ProgressMax);
            }
        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                NotifyOfPropertyChange(() => ProgressValue);
            }
        }

        public object SelectedProgressItem
        {
            get { return _selectedProgressItem; }
            set
            {
                _selectedProgressItem = value;
                NotifyOfPropertyChange(() => SelectedProgressItem);
            }
        }

        public BindableCollection<ProgressItem> ProgressItemList
        {
            get;
            set;
        }

        public ProcViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _cancelTokenSource = new CancellationTokenSource();
            _progress = new Progress<ProgressItem>();
            _progress.ProgressChanged += progress_ProgressChanged;
            this.CancelButtonName = BUTTON_NAME_CANCEL;
            this.ProgressItemList = new BindableCollection<ProgressItem>();
        }

        public void Loaded()
        {
            _fileManager = new FileDispatchManager(this.FolderPath, this.RegexPattern);
            _fileManager.FileDispatchAsync(_cancelTokenSource.Token, _progress)
                .ContinueWith(task => dispatchCompleted(task.Result));
        }

        private void progress_ProgressChanged(object sender, ProgressItem e)
        {
            if (e.MaxCount > 0)
            {
                this.ProgressValue = 0;
                this.ProgressMax = e.MaxCount;
            }
            else
            {
                Debug.WriteLine(e.Message);
                ProgressItemList.Add(e);
                this.ProgressValue = this.ProgressValue + 1;
            }
        }

        private void dispatchCompleted(FileDispatchManager.DispatchResult result)
        {
            switch (result)
            {
                case FileDispatchManager.DispatchResult.Ok:
                    MessageBox.Show("すべてのファイル処理が完了しました");
                    break;
                case FileDispatchManager.DispatchResult.ErrorInput:
                    MessageBox.Show("処理を行うファイルがありません");
                    break;
                case FileDispatchManager.DispatchResult.Cancel:
                    MessageBox.Show("キャンセルされました");
                    break;
            }
            this.CancelButtonName = BUTTON_NAME_BACK;
        }

        public void Cancel()
        {
            if (this.CancelButtonName == BUTTON_NAME_CANCEL)
            {
                //処理キャンセル
                _cancelTokenSource.Cancel();
            }
            else
            {
                //前の画面に戻る
                _navigationService.For<StartViewModel>().Navigate();
            }
        }
        
        public void CopyProgressItem()
        {
            if (SelectedProgressItem is ProgressItem)
            {
                ProgressItem item = (ProgressItem)SelectedProgressItem;
                Clipboard.SetData(DataFormats.Text, item.FilePath);
                MessageBox.Show(Path.GetFileName(item.FilePath) + "のパスをクリップボードにコピーしました。");
            }
        }
    }
}
