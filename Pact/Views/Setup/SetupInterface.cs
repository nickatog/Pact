using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pact
{
    public sealed class SetupInterface
        : ISetupInterface
    {
        ConfigurationData ISetupInterface.RequestInitialConfiguration()
        {
            // create view, show as dialog
            var viewModel = new WelcomeViewModel(@"C:\Program Files (x86)\Hearthstone\Logs\Power.log");
            // on return, gather configuration data
            var view = new WelcomeView(viewModel);

            view.ShowDialog();

            return
                new ConfigurationData()
                {
                    PowerLogFilePath = viewModel.PowerLogFilePath
                };
        }
    }
}
