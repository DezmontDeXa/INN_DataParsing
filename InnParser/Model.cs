using System.Collections.Generic;
using Prism.Mvvm;
using System;
using System.Linq;
using Shared;
using FedResurs;
using Kommersant;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace InnParser
{
    public class Model : BindableBase
    {
        private const string ResultFile = "results.xlsx";
        private bool _isRunning;
        private int _parsedInnsCount;
        private CancellationTokenSource _cts;

        public bool IsRunning
        {
            get => _isRunning;
            internal set => SetProperty(ref _isRunning, value);
        }

        public int ParsedInnsCount
        {
            get => _parsedInnsCount;
            private set => SetProperty(ref _parsedInnsCount, value);
        }

        public event EventHandler<string> OnLogMessage;
        public event EventHandler<ConcurrentBag<InnSource>> OnFinish;


        public void Run(IReadOnlyList<string> inns, string selectedParser, string antiCaptchaKey, int threadsCount)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"Запускаем...\r\nКол-во ИНН: {inns.Count()}\r\nПарсер: {selectedParser}\r\nAntiCaptchaKey: {antiCaptchaKey}\r\nКол-во потоков: {threadsCount}");

                ParsedInnsCount = 0;
                var bag = new ConcurrentBag<InnSource>(inns.Select(x => new InnSource(x)));
                var results = new ConcurrentBag<InnSource>();
                _cts = new CancellationTokenSource();
                List<Task> workers = new List<Task>();
                for (int i = 0; i < threadsCount; i++)
                    workers.Add(Task.Run(() => Parse(CreateParser(selectedParser, antiCaptchaKey), bag, results, _cts.Token)));
                AwaitFinish(workers, bag, results);

                IsRunning = true;
                OnLogMessage?.Invoke(this, $"Запущен.");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Ошибка запуска:: {ex}");
                IsRunning = false;
            }
        }

        private void AwaitFinish(List<Task> workers, ConcurrentBag<InnSource> bag, ConcurrentBag<InnSource> results)
        {
            Task.Run(() =>
            {
                while (workers.Any(x => !x.IsCompleted))
                    Task.Delay(100).Wait();
                OnLogMessage?.Invoke(this, "Парсинг завершен. Сохраняем результаты.");

                foreach (var item in bag)
                    results.Add(item);
                                
                IsRunning = false;
                OnFinish?.Invoke(this, results);
            });
        }

        private void Parse(IInnParser<ParsedDataBase> parser, ConcurrentBag<InnSource> bag, ConcurrentBag<InnSource> results, CancellationToken token)
        {
            while(bag.TryTake(out var innSource))
            {
                try
                {
                    if (innSource.Inn.Length != 12) throw new Exception("Длина ИНН должна быть равна 12 символам.");

                    innSource.Success(parser.Parse(innSource.Inn));
                    OnLogMessage?.Invoke(this, $"ИНН {innSource.Inn} - готово");
                }
                catch(Exception ex)
                {
                    innSource.Failed(ex);
                    OnLogMessage?.Invoke(this, $"ИНН: {innSource.Inn} - ошибка.");
                }
                results.Add(innSource);
                ParsedInnsCount++;
                if (token.IsCancellationRequested) 
                    return;
            }
        }

        private IInnParser<ParsedDataBase> CreateParser(string selectedParser, string antiCaptchaKey)
        {
            return selectedParser switch
            {
                "fedresurs.ru" => new FedResursParser<ParsedDataBase>(),
                "kommersant.ru" => new KommersantParser<ParsedDataBase>(antiCaptchaKey),
                _ => throw new NotImplementedException($"Parser with name {selectedParser} non implemented."),
            };
        }

        public void Stop()
        {
            _cts.Cancel();
            OnLogMessage?.Invoke(this, "Отменяем...");
        }

    }

    public class InnSource
    {
        public string Inn { get; }
        public ParsedDataBase Data { get; private set; }
        public Exception Exception { get; private set; }
        public bool Completed { get; private set; } = false;

        public InnSource(string inn)
        {
            Inn = inn;
        }

        public void Success(ParsedDataBase data)
        {
            Data = data;
            Completed = true; 
        }

        public void Failed(Exception ex)
        {
            Exception = ex;
            Completed = true;
        }
    }
}
