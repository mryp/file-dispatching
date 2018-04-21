using Caliburn.Micro;
using FileDispatching.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileDispatching.ViewModels
{
    public class StartViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private string _folderPath;
        private string _regexPattern;

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                NotifyOfPropertyChange(() => FolderPath);
            }
        }

        public string RegexPattern
        {
            get { return _regexPattern; }
            set
            {
                _regexPattern = value;
                NotifyOfPropertyChange(() => RegexPattern);
            }
        }

        public StartViewModel(INavigationService navigationService)
        {
            this._navigationService = navigationService;

            this.FolderPath = "";
            this.RegexPattern = Properties.Settings.Default.RegexPattern;
        }

        public void SetSelectFolder()
        {
            //https://github.com/aybe/Windows-API-Code-Pack-1.1 を使用
            var dialog = new CommonOpenFileDialog("フォルダ選択")
            {
                IsFolderPicker = true,
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.FolderPath = dialog.FileName;
            }
        }

        public void Start()
        {
            if (!Directory.Exists(this.FolderPath))
            {
                showError("指定したフォルダが見つかりません");
                return;
            }
            if (!FileDispatchManager.IsTargetRegex(this.RegexPattern))
            {
                showError("サブフォルダ名正規表現が正しく指定されていません\n" +
                    "正規表現のグループ名指定 " + FileDispatchManager.GetRegexGroupName() + "を行ってください");
                return;
            }

            saveSetting();
            _navigationService.For<ProcViewModel>()
                .WithParam(v => v.FolderPath, FolderPath)
                .WithParam(v => v.RegexPattern, RegexPattern)
                .Navigate();
        }

        private void showError(string message)
        {
            MessageBox.Show(message, "エラー"
                , MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void saveSetting()
        {
            Properties.Settings.Default.RegexPattern = this.RegexPattern;
            Properties.Settings.Default.Save();
        }
    }
}
