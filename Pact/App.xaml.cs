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
            IUserPrompt userPrompt = new UserPrompt();

            userPrompt.Display("This is a test!", "OK", () => System.Diagnostics.Debug.WriteLine("Testing!"), "Cancel");

            new App().Run(new MainWindow());
        }
    }
}
