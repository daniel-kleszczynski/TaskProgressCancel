using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainWindowViewModel()
        {
            SynchronousWorkCommand = new RelayCommand(SynchronousWork);
            AsynchronousWorkCommand = new RelayCommand(AsynchronousWork);
            ParallelAsynchronousWorkCommand = new RelayCommand(ParallelAsynchronousWork);
        }

        public ICommand SynchronousWorkCommand { get;  }
        public ICommand AsynchronousWorkCommand { get;  }
        public ICommand ParallelAsynchronousWorkCommand { get;  }


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
            RefreshGui();

            var progressTracker = new Progress<ProgressReport>();
            progressTracker.ProgressChanged += ProgressTracker_ProgressChanged;

            var stopWatch = Stopwatch.StartNew();

            var package = await worker.CreatePackageAsync(ITEMS_COUNT, progressTracker);
            stopWatch.Stop();
            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
        }

        private async void ParallelAsynchronousWork(object obj)
        {
            Worker worker = new Worker();

            Items.Clear();
            ProgressBarBackground = Brushes.LightSalmon;
            RefreshGui();

            var stopWatch = Stopwatch.StartNew();
            var package = await worker.CreatePackageParallelAsync(ITEMS_COUNT);

            stopWatch.Stop();

            foreach (var item in package)
                Items.Add(item);

            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
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
