using Supermarket.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Supermarket.Command;

namespace Supermarket.ViewModels
{
    public class AdministratorViewModel : INotifyPropertyChanged
    {
        public ICommand ProductCommand { get; }
        public ICommand UserCommand { get; }
        public ICommand CatagoriCommand { get; }
        public ICommand SupplierCommand { get; }
        public ICommand ReceiptCommand { get; }
        public ICommand StockCommand { get; }
        public ICommand ViewCommand { get; }

        private void OnProduct(object obj)
        {
            var productWindow = new ProductWindow();
            productWindow.DataContext = new ProductViewModel();
            productWindow.Show();
        }

        private void OnUser()
        {
            var userWindow = new UserWindow();
            userWindow.DataContext = new UserViewModel();
            userWindow.Show();
        }

        private void OnCatagori(object obj)
        {
            var categoriWindow = new CategoriWindow();
            categoriWindow.DataContext = new CategoriViewModel();
            categoriWindow.Show();
        }

        private void OnSupplier(object obj)
        {
            var supplierWindow = new SupplierWindow();
            supplierWindow.DataContext = new SupplierViewModel();
            supplierWindow.Show();
        }

        private void OnReceipt(object obj)
        {
            var receiptWindow = new ReceiptWindow();
            receiptWindow.DataContext = new ReceiptViewModel();
            receiptWindow.Show();
        }

        private void OnStock(object obj)
        {
            var stockWindow = new StockWindow();
            stockWindow.DataContext = new StockViewModel();
            stockWindow.Show();
        }
        
        private void OnView(object obj)
        {
            var viewWindow = new ViewWindow();
            viewWindow.DataContext = new viewModel();
            viewWindow.Show();
        }
        public AdministratorViewModel()
        {
            ProductCommand = new RelayCommand(OnProduct);
            UserCommand = new RelayCommand(OnUser);
            CatagoriCommand = new RelayCommand(OnCatagori);
            SupplierCommand = new RelayCommand(OnSupplier);
            ReceiptCommand = new RelayCommand(OnReceipt);
            StockCommand = new RelayCommand(OnStock);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
