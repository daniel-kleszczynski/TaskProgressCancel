using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TaskProgressAndCancel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private SolidColorBrush _progressBarBackground;
        private int _progressValue;
        private CancellationTokenSource _cancellationSource;
        private bool _isCancelButtonEnabled;

        public MainWindowViewModel()
        {
            SynchronousWorkCommand = new RelayCommand(SynchronousWork);
            AsynchronousWorkCommand = new RelayCommand(AsynchronousWork);
            ParallelAsynchronousWorkCommand = new RelayCommand(ParallelAsynchronousWork);
            ParallelSynchronousWorkCommand = new RelayCommand(ParallelSynchronousWork);
            CancelWorkCommand = new RelayCommand(CancelWork);
            StartWorkCommand = new RelayCommand(StartWork);

            PopulateWorkTypeDropdown();
        }

        public ICommand SynchronousWorkCommand { get;  }
        public ICommand AsynchronousWorkCommand { get;  }
        public ICommand ParallelAsynchronousWorkCommand { get;  }
        public ICommand ParallelSynchronousWorkCommand { get;  }
        public ICommand CancelWorkCommand { get; }
        public ICommand StartWorkCommand { get;  }

        public int ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }


        public bool IsCancelButtonEnabled
        {
            get { return _isCancelButtonEnabled; }
            set { SetProperty(ref _isCancelButtonEnabled, value); }
        }


        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();
        public int ItemsCount { get; set; } = 10;
        public List<WorkTypeWrapper> WorkTypes { get; set; }

        public WorkTypeWrapper SelectedWorkType { get; set; }

        private void StartWork(object obj)
        {
            SelectedWorkType.Command.Execute(obj);
        }

        private void SynchronousWork(object obj)
        {
            Worker worker = new Worker();

            GuiSetup();

            var stopWatch = Stopwatch.StartNew();
            var package = worker.CreatePackage(ItemsCount);

            stopWatch.Stop();

            foreach (var item in package)
                Items.Add(item);

            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
            ProgressValue = package.Count * 100 / ItemsCount;
        }

        private async void AsynchronousWork(object obj)
        {
            Worker worker = new Worker();

            GuiSetup();

            var progressTracker = new Progress<ProgressReport>();
            progressTracker.ProgressChanged += ProgressTracker_ProgressChanged;

            _cancellationSource = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();

            try
            {
                var package = await worker.CreatePackageAsync(ItemsCount, progressTracker, 
                    _cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                Items.Add($"Work has been cancelled.");
            }
            finally
            {
                stopWatch.Stop();
                Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
                _cancellationSource.Dispose();
                IsCancelButtonEnabled = false;
            }
        }

        private async void ParallelAsynchronousWork(object obj)
        {
            GuiSetup();

            Worker worker = new Worker();
            var stopWatch = Stopwatch.StartNew();

            var progressTracker = new Progress<ProgressReport>();
            progressTracker.ProgressChanged += ProgressTracker_ProgressChanged;

            _cancellationSource = new CancellationTokenSource();


            var package = await worker.CreatePackageParallelAsync(ItemsCount, progressTracker,
                _cancellationSource.Token);

            stopWatch.Stop();
            IsCancelButtonEnabled = false;
            
            if (Items.Count < ItemsCount)
                Items.Add($"Work has been cancelled.");

            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");

        }

        private void ParallelSynchronousWork(object obj)
        {
            Worker worker = new Worker();

            GuiSetup();

            var stopWatch = Stopwatch.StartNew();
            var package = worker.CreatePackageParallelSync(ItemsCount);

            stopWatch.Stop();

            foreach (var item in package)
                Items.Add(item);

            Items.Add($"Execution time: {stopWatch.ElapsedMilliseconds}");
            ProgressValue = package.Count * 100 / ItemsCount;
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

        private void GuiSetup()
        {
            Items.Clear();
            ProgressValue = 0;
            IsCancelButtonEnabled = true;
            RefreshGui();
        }

        private void PopulateWorkTypeDropdown()
        {
            var workTypes = typeof(MainWindowViewModel).GetProperties()
                .Where(p => IsWorkCommandProperty(p))
                .Select(p => new WorkTypeWrapper(p, this));

            WorkTypes = workTypes.ToList();
        }

        private bool IsWorkCommandProperty(PropertyInfo property)
        {
            const string TYPE_POSTFIX = "WorkCommand";

            return typeof(ICommand).IsAssignableFrom(property.PropertyType) &&
                property.Name.EndsWith(TYPE_POSTFIX) && 
                !property.Name.Equals(nameof(StartWorkCommand)) &&
                !property.Name.Equals(nameof(CancelWorkCommand));
        }
    }
}
