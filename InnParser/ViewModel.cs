using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;

namespace InnParser
{
    public class ViewModel : BindableBase
    {
        #region Private fields
        private readonly Model _model = new Model();
        private readonly ExcelHelper _excel = new ExcelHelper();
        private string _antiCaptchaKey;
        private string _selectedParser = "fedresurs.ru";
        private int _threadsCount = 10;
        private List<string> inns;
        private string _logText;
        private bool _isRunning;
        private DelegateCommand _importINNsCommand;
        private DelegateCommand _runStopCommand;
        #endregion

        #region Public fields
        public string AntiCaptchaKey
        {
            get => _antiCaptchaKey;
            set => SetProperty(ref _antiCaptchaKey, value);
        }
        public List<string> Parsers { get; } = new List<string>()
        {
            "fedresurs.ru",
            "kommersant.ru"
        };
        public string SelectedParser
        {
            get => _selectedParser;
            set => SetProperty(ref _selectedParser, value);
        }
        public int ParsedInnsCount
        {
            get => _model.ParsedInnsCount;
        }
        public int ThreadsCount
        {
            get => _threadsCount;
            set => SetProperty(ref _threadsCount, value);
        }
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }
        public bool IsRunning
        {
            get => _model.IsRunning;
        }
        public IReadOnlyList<string> Inns
        {
            get => inns;
            private set => SetProperty(ref inns, new List<string>(value));
        }
        public ICommand ImportINNsCommand
            => _importINNsCommand ??= new DelegateCommand(ImportINNs);
        public ICommand RunStopCommand
            => _runStopCommand ??= new DelegateCommand(RunStop);
        #endregion

        public ViewModel()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            _model.OnLogMessage += (s, e) => AppendLog(e);
            _model.OnFinish += OnFinish;
            AntiCaptchaKey = ConfigurationManager.AppSettings["AntiCaptchaKey"];
        }

        private void OnFinish(object? sender, ConcurrentBag<InnSource> results)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = "Result.xlsx";
                sfd.Title = "Сохранение файла результата";
                sfd.DefaultExt = ".xlsx";
                if (sfd.ShowDialog() == true)
                {
                    _excel.SaveResults(results, sfd.FileName);
                    AppendLog($"Результаты сохранены в файл {sfd.FileName}.");

                    var result = MessageBox.Show("Результаты парсинга сохранены. Открыть файл?", "Открыть файл", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        OpenResults(sfd.FileName);

                }
                else
                {
                    var result = MessageBox.Show("Результаты парсинга будут утеряны. Продолжить?", "Потеря данных", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                        OnFinish(sender, results);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Ошибка при сохранении: {ex}.");
            }

            Inns = new List<string>();
            AppendLog($"Список ИНН очищен.");
        }

        private void OpenResults(string file)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = file,
                UseShellExecute = true
            });
        }

        private void ImportINNs()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel-файл(*.xlsx)|*.xlsx";
            if (ofd.ShowDialog() == true)
            {
                Inns = _excel.GetINNsFromFile(ofd.FileName);
                var badInns = Inns.Where(x => x.Length != 12).ToList();
                AppendLog($"Загружено {Inns?.Count} ИНН");
                Inns = Inns.Except(badInns).ToList();

                if (badInns.Count > 0)
                {
                    AppendLog($"Обнаружены неверные ИНН:");
                    foreach (var inn in badInns)
                        AppendLog($"\t{inn}");

                    var result = MessageBox.Show($"Обнаружено {badInns.Count} неверных ИНН. Продолжить?\r\n" +
                        $"Да - Продолжить выполнение. Неверные ИНН будут помечаны в выходном файле.\r\n" +
                        $"Нет - Отменить добавление всех ИНН", "Не верные ИНН", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    if (result == MessageBoxResult.No)
                    {
                        Inns = new List<string>();
                        AppendLog($"Список ИНН очищен.");
                    }
                    else
                    {
                        AppendLog($"Загружено {Inns?.Count} ИНН. Из которых {badInns.Count} неверных.");
                    }
                }

                //Inns = Inns.Take(100).ToList();
                //AppendLog($"Демо-версия: Оставили только 100 ИНН.");
            }
        }

        private void AppendLog(string msg)
        {
            LogText += $"{DateTime.Now}: {msg}\r\n";
        }

        private void RunStop()
        {
            if (IsRunning)
            {
                _model.Stop();
                return;
            }
            if (Inns == null || Inns.Count == 0)
            {
                AppendLog("Сначала добавить ИНН");
                return;
            }

            ConfigurationManager.AppSettings["AntiCaptchaKey"] = AntiCaptchaKey;
            _model.Run(Inns, SelectedParser, AntiCaptchaKey, ThreadsCount);
        }
    }
}
