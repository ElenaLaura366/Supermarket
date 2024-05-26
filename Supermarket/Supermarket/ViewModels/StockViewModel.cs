using Supermarket.Command;
using Supermarket.DataService;
using Supermarket.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Supermarket.ViewModels
{
    public class StockViewModel : INotifyPropertyChanged
    {
        private readonly StockDataService _dataService;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Stock> Stocks { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<string> Units { get; set; }

        private Stock _selectedStock;
        private Product _selectedProduct;

        public Stock SelectedStock
        {
            get => _selectedStock;
            set
            {
                if (_selectedStock != value)
                {
                    _selectedStock = value;
                    OnPropertyChanged(nameof(SelectedStock));
                }
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged(nameof(SelectedProduct));
                }
            }
        }

        private decimal _newStockQuantity;
        public decimal NewStockQuantity
        {
            get => _newStockQuantity;
            set
            {
                if (_newStockQuantity != value)
                {
                    _newStockQuantity = value;
                    OnPropertyChanged(nameof(NewStockQuantity));
                }
            }
        }

        private decimal _newStockPrice;
        public decimal NewStockPrice
        {
            get => _newStockPrice;
            set
            {
                if (_newStockPrice != value)
                {
                    _newStockPrice = value;
                    OnPropertyChanged(nameof(NewStockPrice));
                }
            }
        }

        private string _newStockUnit;
        public string NewStockUnit
        {
            get => _newStockUnit;
            set
            {
                if (_newStockUnit != value)
                {
                    _newStockUnit = value;
                    OnPropertyChanged(nameof(NewStockUnit));
                }
            }
        }

        private DateTime _newStockSupplyDate;
        public DateTime NewStockSupplyDate
        {
            get => _newStockSupplyDate;
            set
            {
                if (_newStockSupplyDate != value)
                {
                    _newStockSupplyDate = value;
                    OnPropertyChanged(nameof(NewStockSupplyDate));
                }
            }
        }

        private DateTime _newStockExpirationDate;
        public DateTime NewStockExpirationDate
        {
            get => _newStockExpirationDate;
            set
            {
                if (_newStockExpirationDate != value)
                {
                    _newStockExpirationDate = value;
                    OnPropertyChanged(nameof(NewStockExpirationDate));
                }
            }
        }

        public ICommand LoadCommand { get; private set; }
        public ICommand AddCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public StockViewModel()
        {
            _dataService = new StockDataService();
            Stocks = new ObservableCollection<Stock>(_dataService.GetAllStocks());
            Products = new ObservableCollection<Product>(_dataService.GetAllProducts());
            Units = new ObservableCollection<string> { "Kg", "Litri", "Bucati" };

            NewStockSupplyDate = DateTime.Now;
            NewStockExpirationDate = DateTime.Now.AddYears(1);

            LoadCommand = new RelayCommand(LoadStocks);
            AddCommand = new RelayCommand(AddStock);
            EditCommand = new RelayCommand(EditStock);
            DeleteCommand = new RelayCommand(DeleteStock, CanDeleteStock);
        }

        private void LoadStocks()
        {
            Stocks.Clear();
            var stocks = _dataService.GetAllStocks();
            foreach (var stock in stocks)
            {
                Stocks.Add(stock);
            }
        }

        private void AddStock()
        {
            if (SelectedProduct != null)
            {
                if (NewStockSupplyDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue ||
                    NewStockExpirationDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue)
                {
                    throw new Exception("Date must be between 1/1/1753 and 12/31/9999.");
                }

                var newStock = new Stock
                {
                    ID_Produs = SelectedProduct.ID,
                    ProductName = SelectedProduct.Name,
                    Cantitate = NewStockQuantity,
                    Unitate_Masura = NewStockUnit,
                    Data_Aprovizionare = NewStockSupplyDate,
                    Data_Expirare = NewStockExpirationDate,
                    Pret_Achizitie = NewStockPrice,
                    Pret_Vanzare = NewStockPrice,
                    IsActive = true
                };

                _dataService.AddStock(newStock);
                Stocks.Add(newStock);
            }
        }

        private void EditStock()
        {
            if (SelectedStock != null)
            {
                _dataService.UpdateStockPrice(SelectedStock.ID, SelectedStock.Pret_Vanzare);
                LoadStocks();
            }
        }

        private bool CanDeleteStock()
        {
            return SelectedStock != null;
        }

        private void DeleteStock()
        {
            if (SelectedStock != null)
            {
                _dataService.DeleteStock(SelectedStock);
                SelectedStock.IsActive = false;
                LoadStocks();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
