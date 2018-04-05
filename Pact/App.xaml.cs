using System;
using System.Windows;

namespace Pact
{
    public partial class App : Application
    {
        private App()
        {
            InitializeComponent();
        }

        [STAThread]
        public static void Main()
        {
            new App().Run(new MainWindow());
        }
    }
}
