using Supermarket.ViewModels;
using System.Windows;

namespace Supermarket.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
        }
    }
}
