using Supermarket.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

namespace Supermarket.ViewModels
{
    public class viewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Supplier> producers;
        private ObservableCollection<ProductCategoryGroup> productsByCategory;
        private ObservableCollection<CategoryValueGroup> categoryValues;
        private ObservableCollection<User> users;
        private Supplier selectedProducer;
        private CategoryValueGroup selectedCategory;
        private User selectedUser;
        private DateTime selectedDate;
        private decimal totalValueOfSelectedCategory;
        private decimal totalAmountCollected;
        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

        public ObservableCollection<Supplier> Producers
        {
            get { return producers; }
            set { producers = value; OnPropertyChanged("Producers"); }
        }

        public ObservableCollection<ProductCategoryGroup> ProductsByCategory
        {
            get { return productsByCategory; }
            set { productsByCategory = value; OnPropertyChanged("ProductsByCategory"); }
        }

        public ObservableCollection<CategoryValueGroup> CategoryValues
        {
            get { return categoryValues; }
            set { categoryValues = value; OnPropertyChanged("CategoryValues"); }
        }

        public ObservableCollection<User> Users
        {
            get { return users; }
            set { users = value; OnPropertyChanged("Users"); }
        }

        public Supplier SelectedProducer
        {
            get { return selectedProducer; }
            set
            {
                selectedProducer = value;
                OnPropertyChanged("SelectedProducer");
                LoadProductsForProducer();
            }
        }

        public CategoryValueGroup SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                OnPropertyChanged("SelectedCategory");
                TotalValueOfSelectedCategory = selectedCategory != null ? selectedCategory.TotalValue : 0;
            }
        }

        public User SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                OnPropertyChanged("SelectedUser");
                LoadTotalAmountCollected();
            }
        }

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                OnPropertyChanged("SelectedDate");
                LoadTotalAmountCollected();
            }
        }

        public decimal TotalValueOfSelectedCategory
        {
            get { return totalValueOfSelectedCategory; }
            set
            {
                totalValueOfSelectedCategory = value;
                OnPropertyChanged("TotalValueOfSelectedCategory");
            }
        }

        public decimal TotalAmountCollected
        {
            get { return totalAmountCollected; }
            set
            {
                totalAmountCollected = value;
                OnPropertyChanged("TotalAmountCollected");
            }
        }

        public viewModel()
        {
            Producers = new ObservableCollection<Supplier>();
            ProductsByCategory = new ObservableCollection<ProductCategoryGroup>();
            CategoryValues = new ObservableCollection<CategoryValueGroup>();
            Users = new ObservableCollection<User>();
            LoadProducers();
            LoadCategoryValues();
            LoadUsers();
            SelectedDate = DateTime.Today;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadProducers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT ID_Producator, Nume_Producator FROM Producatori", connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Producers.Add(new Supplier
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
        }

        private void LoadProductsForProducer()
        {
            if (SelectedProducer == null) return;

            ProductsByCategory.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"
                    SELECT p.Nume_Produs, c.Nume_Categorie
                    FROM Produse p
                    JOIN Categorii c ON p.ID_Categorie = c.ID_Categorie
                    WHERE p.ID_Producator = @ProducerId", connection);
                command.Parameters.AddWithValue("@ProducerId", SelectedProducer.Id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var query = from r in reader.Cast<System.Data.Common.DbDataRecord>()
                                group r by r["Nume_Categorie"] into g
                                select new ProductCategoryGroup
                                {
                                    Category = g.Key.ToString(),
                                    Products = new ObservableCollection<string>(g.Select(x => x["Nume_Produs"].ToString()))
                                };

                    foreach (var item in query)
                    {
                        ProductsByCategory.Add(item);
                    }
                }
            }
        }

        private void LoadCategoryValues()
        {
            CategoryValues.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"
                    SELECT c.Nume_Categorie, 
                           SUM(CASE 
                                WHEN o.Procent_Reducere IS NOT NULL 
                                THEN s.Pret_Vanzare * (1 - o.Procent_Reducere / 100) * s.Cantitate
                                ELSE s.Pret_Vanzare * s.Cantitate
                                END) AS TotalValue
                    FROM Produse p
                    JOIN Categorii c ON p.ID_Categorie = c.ID_Categorie
                    JOIN Stocuri s ON p.ID_Produs = s.ID_Produs
                    LEFT JOIN Oferte o ON p.ID_Produs = o.ID_Produs AND GETDATE() BETWEEN o.Data_Start AND o.Data_End
                    GROUP BY c.Nume_Categorie", connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CategoryValues.Add(new CategoryValueGroup
                        {
                            Category = reader.GetString(0),
                            TotalValue = Convert.ToDecimal(reader["TotalValue"])
                        });
                    }
                }
            }
        }

        private void LoadUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT ID_Utilizator, Nume_Utilizator FROM Utilizatori WHERE Tip_Utilizator = 'Casier' AND IsActive = 1", connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Users.Add(new User
                        {
                            ID = reader.GetInt32(0),
                            username = reader.GetString(1)
                        });
                    }
                }
            }
        }

        private void LoadTotalAmountCollected()
        {
            if (SelectedUser == null || SelectedDate == null) return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"
                    SELECT SUM(Suma_Incasata)
                    FROM Bonuri_de_Casa
                    WHERE ID_Casier = 1 AND CAST(Data_Eliberare AS DATE) = @SelectedDate", connection);
                command.Parameters.AddWithValue("@UserId", SelectedUser.ID);
                command.Parameters.AddWithValue("@SelectedDate", SelectedDate.Date);

                var result = command.ExecuteScalar();
                TotalAmountCollected = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }
    }

    public class ProductCategoryGroup
    {
        public string Category { get; set; }
        public ObservableCollection<string> Products { get; set; }
    }

    public class CategoryValueGroup
    {
        public string Category { get; set; }
        public decimal TotalValue { get; set; }
    }
}
