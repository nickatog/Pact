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
                new List<IPowerLogBlockParser>
                {
                    new Events.Parsers.PowerLog.BlockParsers.CreateGame(),
                    new Events.Parsers.PowerLog.BlockParsers.PlayBlockEventParser(),
                    new Events.Parsers.PowerLog.BlockParsers.Trigger()
                };
            // @"C:\Program Files (x86)\Hearthstone\Logs\Power.log",
            _eventStream =
                new PowerLogEventStream(
                    @"C:\Users\Nicholas Anderson\Desktop\Power2.log",
                    new PowerLogParser(eventParsers));

            Task.Run(
                async () =>
                {
                    while (true)
                    {
                        object @event = await _eventStream.ReadNext();

                        System.Diagnostics.Debug.WriteLine($"{DateTime.Now}");
                    }
                });
        }
    }
}
