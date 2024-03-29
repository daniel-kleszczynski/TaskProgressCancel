﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskProgressAndCancel
{
    public class Worker
    {
        private List<string> _package;
        private int _index;
        private object _lock = new object();

        public string CreateItem()
        {
            string outcome = string.Empty;
            Thread.Sleep(1000);

            lock (_lock)
            {
                outcome = $"Item nr {(_index).ToString()} collected." + Environment.NewLine;
                var temp = _index + 1;
                Thread.Sleep(50);
                _index = temp;
                return outcome;
            }
        }

        public async Task<string> CreateItemAsync()
        {
            return await Task.Run(() =>
            {
                string outcome = string.Empty;
                Thread.Sleep(1000);

                lock (_lock)
                {
                    outcome = $"Item nr {(_index).ToString()} collected." + Environment.NewLine;
                    var temp = _index + 1;
                    Thread.Sleep(5);
                    _index = temp;
                    return outcome;
                }
            });

        }


        public List<string> CreatePackage(int size)
        {
            _package = new List<string>();
            _index = 0;

            for (int i = 0; i < size; i++)
            {
                _package.Add(CreateItem());
            }

            return _package;
        }

        public async Task<List<string>> CreatePackageAsync(
            int size,
            IProgress<ProgressReport> progress,
            CancellationToken cancellationToken)
        {
            _package = new List<string>();
            _index = 0;

            for (int i = 0; i < size; i++)
            {
                var item = await CreateItemAsync();
                cancellationToken.ThrowIfCancellationRequested();
                _package.Add(item);

                var report = GenerateProgressReport(_package.Count, size);
                progress.Report(report);
            }

            return _package;
        }

        public async Task<List<string>> CreatePackageParallelAsyncObsolete(int size)
        {
            _package = new List<string>();
            _index = 0;

            Task<string>[] tasks = new Task<string>[size];

            for (int i = 0; i < size; i++)
            {
                tasks[i] = CreateItemAsync();
            }

            await Task.WhenAll(tasks).ContinueWith((t) =>
            {
                //t.Result contains array of string (composite Task<string>)
                foreach (var result in t.Result)
                {
                    _package.Add(result);
                }
            });

            return _package;
        }

        public async Task<List<string>> CreatePackageParallelAsync(int size,
            IProgress<ProgressReport> progressTracker,
            CancellationToken cancellationToken)
        {
            _package = new List<string>();
            _index = 0;

            await Task.Run(() =>
            {
                Parallel.For(0, size, (i) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var item = CreateItem();
                    _package.Add(item);

                    var progressReport = GenerateProgressReport(_package.Count, size);
                    progressTracker.Report(progressReport);
                });
            });

            return _package;
        }

        public List<string> CreatePackageParallelSync(int size)
        {
            _package = new List<string>();
            _index = 0;

            Parallel.For(0, size, (i) =>
            {
                _package.Add(this.CreateItem());
            });
            
            return _package;
        }

        private ProgressReport GenerateProgressReport(int currentSize, int finalSize)
        {
            var percentageCompleted = currentSize * 100 / finalSize;
            return new ProgressReport
            {
                NewItemCollected = _package.Last(),
                PercentageCompleted = percentageCompleted
            };
        }
    }
}
