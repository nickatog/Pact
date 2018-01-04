using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Pact
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IEventStream _eventStream;

        public MainWindow()
        {
            InitializeComponent();

            var eventParsers =
                new List<IGameStateDebugEventParser>
                {
                    new EventParsers.PowerLog.GameStateDebug.Block(),
                    new EventParsers.PowerLog.GameStateDebug.ShowEntity()
                };
            // @"C:\Program Files (x86)\Hearthstone\Logs\Power.log",
            // @"C:\Users\Nicholas Anderson\Desktop\Power2.log",
            _eventStream =
                new PowerLogEventStream(
                    @"C:\Program Files (x86)\Hearthstone\Logs\Power.log",
                    new GameStateDebugPowerLogEventParser(eventParsers));

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        try
                        {
                            object @event = await _eventStream.ReadNext();

                            System.Diagnostics.Debug.WriteLine($"{DateTime.Now} - {@event}");
                        } catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                        }
                    }
                });
        }
    }
}
