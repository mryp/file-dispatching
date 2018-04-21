using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileDispatching.Models
{
    public class FileDispatchManager
    {
        public const string REGEX_GROUP_NAME = "name";

        public enum DispatchResult
        {
            Ok,
            Cancel,
            ErrorInput,
        }

        private string _dirPath;
        private Regex _regex;

        public FileDispatchManager(string dirPath, string regexPattern)
        {
            _dirPath = dirPath;
            _regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public async Task<DispatchResult> FileDispatchAsync(CancellationToken cancelToken
            , IProgress<ProgressItem> progress)
        {
            DispatchResult result = DispatchResult.ErrorInput;
            try
            {
                await Task.Run(() =>
                {
                    if (!Directory.Exists(_dirPath))
                    {
                        result = DispatchResult.ErrorInput;
                        return;
                    }
                    string[] files = Directory.GetFiles(_dirPath);
                    if (files.Length == 0)
                    {
                        result = DispatchResult.ErrorInput;
                        return;
                    }

                    progress?.Report(new ProgressItem() { MaxCount = files.Length });
                    foreach (string file in files)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        ProgressItem progItem = dispatchFile(file);
                        progress?.Report(progItem);
                    }

                    result = DispatchResult.Ok;
                });
            }
            catch (OperationCanceledException cancelEx)
            {
                Debug.WriteLine($"RunDispatch キャンセル例外={cancelEx.Message}");
                result = DispatchResult.Cancel;
            }

            return result;
        }

        private ProgressItem dispatchFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            Match match = _regex.Match(fileName);
            if (!match.Success)
            {
                return new ProgressItem()
                {
                    FilePath = filePath,
                    Message = "サブフォルダ名正規表現に一致するファイル名ではありません",
                    IsSuccess = false,
                };
            }
            string subDirName = match.Groups?["name"].Value;
            if (string.IsNullOrEmpty(subDirName))
            {
                return new ProgressItem()
                {
                    FilePath = filePath,
                    Message = "サブフォルダ名正規表現で一致するグループが取得できません",
                    IsSuccess = false,
                };
            }
            string subDirPath = Path.Combine(Path.GetDirectoryName(filePath), subDirName);
            if (File.Exists(subDirPath))
            {
                return new ProgressItem()
                {
                    FilePath = filePath,
                    Message = "サブフォルダと同名のファイルが既に存在しています",
                    IsSuccess = false,
                };
            }
            string moveFilePath = Path.Combine(subDirPath, fileName);
            if (File.Exists(moveFilePath))
            {
                return new ProgressItem()
                {
                    FilePath = filePath,
                    Message = "移動先に同名のファイルが既に存在しています",
                    IsSuccess = false,
                };
            }

            try
            {
                if (!Directory.Exists(subDirPath))
                {
                    Directory.CreateDirectory(subDirPath);
                }
                File.Move(filePath, moveFilePath);
            }
            catch (Exception e)
            {
                return new ProgressItem()
                {
                    FilePath = filePath,
                    Message = $"ファイルの移動に失敗しました e={e.Message}",
                    IsSuccess = false,
                };
            }

            return new ProgressItem()
            {
                FilePath = filePath,
                Message = "成功",
                IsSuccess = true,
            };
        }

        public static bool IsTargetRegex(string text)
        {
            if (!text.Contains(GetRegexGroupName()))
            {
                return false;
            }

            return true;
        }

        public static string GetRegexGroupName()
        {
            return "?<" + REGEX_GROUP_NAME + ">";
        }
    }
}
