using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TaskProgressAndCancel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const int ITEMS_COUNT = 5;

        private SolidColorBrush _progressBarBackground;
        private int _progressValue;
        private CancellationTokenSource _cancellationSource;

        public MainWindowViewModel()
        {
            SynchronousWorkCommand = new RelayCommand(SynchronousWork);
            AsynchronousWorkCommand = new RelayCommand(AsynchronousWork);
            ParallelAsynchronousWorkCommand = new RelayCommand(ParallelAsynchronousWork);
            CancelWorkCommand = new RelayCommand(CancelWork);
        }

        public ICommand SynchronousWorkCommand { get;  }
        public ICommand AsynchronousWorkCommand { get;  }
        public ICommand ParallelAsynchronousWorkCommand { get;  }
        public ICommand CancelWorkCommand { get;  }

        public SolidColorBrush ProgressBarBackground
        {
            get { return _progressBarBackground; }
            set { SetProperty(ref _progressBarBackground, value); }
        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }


        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();

        private void SynchronousWork(object obj)
        {
            Worker worker = new Worker();

            Items.Clear();
            ProgressBarBackground = Brushes.LightSalmon;
            ProgressValue = 0;
            RefreshGui();

            var stopWatch = Stopwatch.StartNew();
            var package = worker.CreatePackage(ITEMS_COUNT);

            stopWatch.Stop();

            foreach (var item in package)
                Items.Add(item);

            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
        }

        private async void AsynchronousWork(object obj)
        {
            Worker worker = new Worker();

            Items.Clear();
            ProgressBarBackground = Brushes.LightSalmon;
            ProgressValue = 0;
            RefreshGui();

            var progressTracker = new Progress<ProgressReport>();
            progressTracker.ProgressChanged += ProgressTracker_ProgressChanged;

            _cancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();

            try
            {
                var package = await worker.CreatePackageAsync(ITEMS_COUNT, progressTracker, 
                    _cancellationSource.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                stopWatch.Stop();
                Items.Add($"Work has been cancelled.");
                Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
                _cancellationSource.Dispose();
            }
        }

        private async void ParallelAsynchronousWork(object obj)
        {
            Items.Clear();
            ProgressValue = 0;
            ProgressBarBackground = Brushes.LightSalmon;
            RefreshGui();

            Worker worker = new Worker();
            var stopWatch = Stopwatch.StartNew();
            var package = await worker.CreatePackageParallelAsync(ITEMS_COUNT);

            stopWatch.Stop();

            foreach (var item in package)
                Items.Add(item);

            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
        }

        private void CancelWork(object obj)
        {
            _cancellationSource?.Cancel();
        }

        private void ProgressTracker_ProgressChanged(object sender, ProgressReport e)
        {
            ProgressValue = e.PercentageCompleted;
            Items.Add(e.NewItemCollected);
        }

        private void RefreshGui()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ApplicationIdle);
        }
    }
}
