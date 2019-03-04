﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TaskProgressAndCancel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Background = Brushes.LightSalmon;
            //Refresh(this);

            //Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ApplicationIdle);

            (DataContext as MainWindowViewModel).SynchronousWorkCommand.Execute(new object());
        }
    }
}

//Command="{Binding SynchronousWorkCommand}"