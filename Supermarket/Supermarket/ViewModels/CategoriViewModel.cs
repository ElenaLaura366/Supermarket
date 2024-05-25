using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Data.SqlClient;
using Supermarket.Command;

namespace Supermarket.ViewModels
{
    public class CategoriViewModel : INotifyPropertyChanged
    {
        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
        private string _selectedCategory;
        private string _categoryName;

        public CategoriViewModel()
        {
            LoadCategories();
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged(nameof(CategoryName));
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                CategoryName = _selectedCategory;
            }
        }

        public ICommand AddCategoryCommand => new RelayCommand(AddCategory);
        public ICommand EditCategoryCommand => new RelayCommand(EditCategory);
        public ICommand DeleteCategoryCommand => new RelayCommand(DeleteCategory);

        private void LoadCategories()
        {
            Categories.Clear();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Nume_Categorie FROM Categorii WHERE Activ = 1", connection); // Încărcăm doar categoriile active
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Categories.Add(reader.GetString(0));
                    }
                }
            }
        }

        private void AddCategory()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Categorii (Nume_Categorie, Activ) VALUES (@CategoryName, 1)", connection);
                command.Parameters.AddWithValue("@CategoryName", CategoryName);
                command.ExecuteNonQuery();
            }
            LoadCategories();
        }

        private void EditCategory()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Categorii SET Nume_Categorie = @CategoryName WHERE Nume_Categorie = @OldCategoryName AND Activ = 1", connection);
                command.Parameters.AddWithValue("@CategoryName", CategoryName);
                command.Parameters.AddWithValue("@OldCategoryName", SelectedCategory);
                command.ExecuteNonQuery();
            }
            LoadCategories();
        }

        private void DeleteCategory()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Categorii SET Activ = 0 WHERE Nume_Categorie = @CategoryName", connection);
                command.Parameters.AddWithValue("@CategoryName", SelectedCategory);
                command.ExecuteNonQuery();
            }
            LoadCategories();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
