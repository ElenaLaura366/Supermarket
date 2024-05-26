using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Supermarket.Command;
using Supermarket.Views;

namespace Supermarket.ViewModels
{
    public class CasierViewModel : INotifyPropertyChanged
    {
        public ICommand ReceiptCommand { get; }
        public ICommand ProductCommand { get; }

        public CasierViewModel()
        {
            ReceiptCommand = new RelayCommand(Receipt);
            ProductCommand = new RelayCommand(Product);
        }

        private void Receipt(object obj)
        {
            var receiptSellerWindow = new ReceiptSellerWindow();
            receiptSellerWindow.DataContext = new ReceiptSellerViewModel();
            receiptSellerWindow.Show();
        }

        private void Product(object obj)
        {
            var productSellerWindow = new ProductSellerWindow();
            productSellerWindow.DataContext = new ProductSellerViewModel();
            productSellerWindow.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
