﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();

        private void SynchronousWork(object obj)
        {
            Worker worker = new Worker();

            Items.Clear();
            ProgressBarBackground = Brushes.LightSalmon;
            RefreshGui();

            var package = worker.CreatePackage(ITEMS_COUNT);

            foreach (var item in package)
                Items.Add(item);
        }

        private async void AsynchronousWork(object obj)
        {
            
            Worker worker = new Worker();

            Items.Clear();
            ProgressBarBackground = Brushes.LightSalmon;
            RefreshGui();

            var package = await worker.CreatePackageAsync(ITEMS_COUNT);

            foreach (var item in package)
                Items.Add(item);
        }

        private async void ParallelAsynchronousWork(object obj)
        {
            Worker worker = new Worker();

            Items.Clear();
            ProgressBarBackground = Brushes.LightSalmon;
            RefreshGui();

            var package = await worker.CreatePackageParallelAsync(ITEMS_COUNT);

            foreach (var item in package)
                Items.Add(item);
        }

        private void RefreshGui()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ApplicationIdle);

        }
    }
}