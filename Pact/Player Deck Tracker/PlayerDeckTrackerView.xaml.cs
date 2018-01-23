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

namespace Pact
{
    public partial class PlayerDeckTrackerView
        : Window
    {
        private static PlayerDeckTrackerView _window = new PlayerDeckTrackerView() { Owner = MainWindow.Window };

        public PlayerDeckTrackerView()
        {
            InitializeComponent();
        }

        public static PlayerDeckTrackerView GetWindowFor(
            PlayerDeckTrackerViewModel viewModel)
        {
            _window.DataContext = viewModel;

            return _window;
        }
    }
}
