using System;
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
using System.Windows.Shapes;

namespace Pact
{
    public partial class UserPromptView
        : Window
    {
        private readonly string _cancelText;
        private readonly string _confirmText;
        private readonly string _message;

        public UserPromptView(
            string message,
            string confirmText,
            string cancelText)
        {
            InitializeComponent();

            _cancelText = cancelText;
            _confirmText = confirmText;
            _message = message;

            DataContext = this;
        }

        public string CancelText => _cancelText;

        public string ConfirmText => _confirmText;

        public string Message => _message;
    }
}
