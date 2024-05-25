using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Supermarket.Command;
using Supermarket.Model;
using Supermarket.ViewModels;
using System.Linq;
using System.Diagnostics;

namespace Supermarket.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<Supplier> Producers { get; set; }
        public ObservableCollection<string> ProductNames { get; set; }

        private string _selectedProductName;
        public string SelectedProductName
        {
            get => _selectedProductName;
            set
            {
                if (_selectedProductName != value)
                {
                    _selectedProductName = value;
                    OnPropertyChanged(nameof(SelectedProductName));
                    UpdateSelectedProductDetails(); // Update other details based on selected name
                }
            }
        }
        public Category SelectedCategory { get; set; }
        public Supplier SelectedProducer { get; set; }


        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }
        public ProductViewModel()
        {
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<Category>();
            Producers = new ObservableCollection<Supplier>();
            ProductNames = new ObservableCollection<string>();

            LoadActiveProducts();
            LoadCategories();
            LoadProducers();
            LoadProductNames();
        }

        public void LoadProductNames()
        {
            ProductNames.Clear();
            foreach (var product in Products)
            {
                if (!ProductNames.Contains(product.Name))
                    ProductNames.Add(product.Name);
            }
        }

        private void UpdateSelectedProductDetails()
        {
            SelectedProduct = Products.FirstOrDefault(p => p.Name == SelectedProductName);
            if (SelectedProduct != null)
            {
                // Now also update category and producer based on this new selected product
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedProduct.CategoryId);
                SelectedProducer = Producers.FirstOrDefault(p => p.Id == SelectedProduct.ProducerId);
                OnPropertyChanged(nameof(SelectedCategory));
                OnPropertyChanged(nameof(SelectedProducer));
            }
        }

        public void LoadActiveProducts()
        {
            Products.Clear();
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("SELECT ID_Produs, Nume_Produs, ID_Categorie, ID_Producator, Activ FROM Produse WHERE Activ=1", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products.Add(new Product
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID_Produs")),
                            Name = reader.GetString(reader.GetOrdinal("Nume_Produs")),
                            CategoryId = reader.GetInt32(reader.GetOrdinal("ID_Categorie")),
                            ProducerId = reader.GetInt32(reader.GetOrdinal("ID_Producator")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("Activ"))
                        });
                    }
                }
            }
            LoadProductNames(); // Refresh names after loading products
        }

        // Properties for data binding
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int ProducerId { get; set; }
        public string Description { get; set; }

        // Commands
        public ICommand AddProductCommand => new RelayCommand(AddProduct);
        public ICommand EditProductCommand => new RelayCommand(EditProduct);
        public ICommand DeactivateProductCommand => new RelayCommand(DeactivateProduct);

        private void AddProduct()
        {
            if (SelectedCategory == null || SelectedProducer == null)
            {
                Debug.WriteLine("Category and Producer must be selected.");
                return;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("INSERT INTO Produse (Nume_Produs, ID_Categorie, ID_Producator, Activ) VALUES (@Name, @CategoryId, @ProducerId, 1)", connection);
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@CategoryId", SelectedCategory.Id);
                command.Parameters.AddWithValue("@ProducerId", SelectedProducer.Id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            LoadActiveProducts();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT ID_Categorie, Nume_Categorie FROM Categorii", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var category = new Category
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        };
                        Categories.Add(category);
                    }
                }
            }
            SelectedCategory = Categories.FirstOrDefault(); // Set default selection
        }

        private void LoadProducers()
        {
            Producers.Clear();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT ID_Producator, Nume_Producator, Tara_Origine FROM Producatori", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var supplier = new Supplier
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Country = reader.GetString(2)
                        };
                        Producers.Add(supplier);
                    }
                }
            }
            SelectedProducer = Producers.FirstOrDefault(); // Set default selection
        }

        private void EditProduct()
        {
            if (SelectedProduct == null)
            {
                // Log the error or notify the user that no product has been selected
                Debug.WriteLine("No product selected for editing.");
                return; // Exit the method to prevent the exception
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("UPDATE Produse SET Nume_Produs=@Name, ID_Categorie=@CategoryId, ID_Producator=@ProducerId WHERE ID_Produs=@ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", SelectedProduct.ID);
                command.Parameters.AddWithValue("@Name", SelectedProductName ?? SelectedProduct.Name); // Use SelectedProductName or the existing name if not set
                command.Parameters.AddWithValue("@CategoryId", SelectedCategory?.Id ?? SelectedProduct.CategoryId); // Use selected or existing category
                command.Parameters.AddWithValue("@ProducerId", SelectedProducer?.Id ?? SelectedProduct.ProducerId); // Use selected or existing producer
                connection.Open();
                command.ExecuteNonQuery();
            }
            LoadActiveProducts();
        }

        private void DeactivateProduct()
        {
            if (SelectedProductName == null)
            {
                Debug.WriteLine("No product selected for deletion.");
                return; // Exit the method to prevent exception
            }

            var productToDelete = Products.FirstOrDefault(p => p.Name == SelectedProductName);
            if (productToDelete != null)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand("UPDATE Produse SET Activ=0 WHERE ID_Produs=@ProductId", connection);
                    command.Parameters.AddWithValue("@ProductId", productToDelete.ID);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                Products.Remove(productToDelete); // Optionally remove from local list or refresh list
                ProductNames.Remove(SelectedProductName);
                SelectedProductName = null;
                LoadActiveProducts(); // Optionally reload products to reflect changes
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}