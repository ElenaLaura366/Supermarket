using Supermarket.Command;
using Supermarket.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Supermarket.ViewModels
{
    public class ProductSellerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _searchText;
        private DateTime? _expirationDate;
        private ObservableCollection<Product> _searchResults;
        private bool _isProductNameChecked;
        private bool _isExpirationDateChecked;
        private bool _isProducerChecked;
        private bool _isCategoryChecked;
        private Visibility _expirationDateVisibility;
        private Visibility _searchTextBoxVisibility;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public DateTime? ExpirationDate
        {
            get => _expirationDate;
            set
            {
                _expirationDate = value;
                OnPropertyChanged(nameof(ExpirationDate));
            }
        }

        public ObservableCollection<Product> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged(nameof(SearchResults));
            }
        }

        public bool IsProductNameChecked
        {
            get => _isProductNameChecked;
            set
            {
                _isProductNameChecked = value;
                OnPropertyChanged(nameof(IsProductNameChecked));
                UpdateVisibility();
            }
        }

        public bool IsExpirationDateChecked
        {
            get => _isExpirationDateChecked;
            set
            {
                _isExpirationDateChecked = value;
                OnPropertyChanged(nameof(IsExpirationDateChecked));
                UpdateVisibility();
            }
        }

        public bool IsProducerChecked
        {
            get => _isProducerChecked;
            set
            {
                _isProducerChecked = value;
                OnPropertyChanged(nameof(IsProducerChecked));
                UpdateVisibility();
            }
        }

        public bool IsCategoryChecked
        {
            get => _isCategoryChecked;
            set
            {
                _isCategoryChecked = value;
                OnPropertyChanged(nameof(IsCategoryChecked));
                UpdateVisibility();
            }
        }

        public Visibility ExpirationDateVisibility
        {
            get => _expirationDateVisibility;
            set
            {
                _expirationDateVisibility = value;
                OnPropertyChanged(nameof(ExpirationDateVisibility));
            }
        }

        public Visibility SearchTextBoxVisibility
        {
            get => _searchTextBoxVisibility;
            set
            {
                _searchTextBoxVisibility = value;
                OnPropertyChanged(nameof(SearchTextBoxVisibility));
            }
        }

        public ICommand SearchCommand { get; }

        public ProductSellerViewModel()
        {
            _searchResults = new ObservableCollection<Product>();
            SearchCommand = new RelayCommand(SearchProducts);
            UpdateVisibility();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateVisibility()
        {
            ExpirationDateVisibility = IsExpirationDateChecked ? Visibility.Visible : Visibility.Collapsed;
            SearchTextBoxVisibility = IsProductNameChecked || IsProducerChecked || IsCategoryChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SearchProducts()
        {
            var connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = new StringBuilder("SELECT * FROM Produse WHERE 1=1");

                if (IsProductNameChecked && !string.IsNullOrEmpty(SearchText))
                {
                    query.Append(" AND Nume_Produs LIKE @SearchText");
                }
                else if (IsExpirationDateChecked && ExpirationDate.HasValue)
                {
                    query.Append(" AND ID_Produs IN (SELECT ID_Produs FROM Stocuri WHERE Data_Expirare = @ExpirationDate)");
                }
                else if (IsProducerChecked && !string.IsNullOrEmpty(SearchText))
                {
                    query.Append(" AND ID_Producator IN (SELECT ID_Producator FROM Producatori WHERE Nume_Producator LIKE @SearchText)");
                }
                else if (IsCategoryChecked && !string.IsNullOrEmpty(SearchText))
                {
                    query.Append(" AND ID_Categorie IN (SELECT ID_Categorie FROM Categorii WHERE Nume_Categorie LIKE @SearchText)");
                }

                using (SqlCommand command = new SqlCommand(query.ToString(), connection))
                {
                    if (IsProductNameChecked && !string.IsNullOrEmpty(SearchText))
                    {
                        command.Parameters.AddWithValue("@SearchText", $"%{SearchText}%");
                    }
                    else if (IsExpirationDateChecked && ExpirationDate.HasValue)
                    {
                        command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate.Value);
                    }
                    else if (IsProducerChecked && !string.IsNullOrEmpty(SearchText))
                    {
                        command.Parameters.AddWithValue("@SearchText", $"%{SearchText}%");
                    }
                    else if (IsCategoryChecked && !string.IsNullOrEmpty(SearchText))
                    {
                        command.Parameters.AddWithValue("@SearchText", $"%{SearchText}%");
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        _searchResults.Clear();
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                CategoryId = reader.GetInt32(2),
                                ProducerId = reader.GetInt32(3)
                            };
                            _searchResults.Add(product);
                        }
                    }
                }
            }
        }
    }
}