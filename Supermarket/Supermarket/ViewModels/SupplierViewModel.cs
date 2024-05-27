using Supermarket.Command;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows.Input;
using Supermarket.Model;

namespace Supermarket.ViewModels
{
    public class SupplierViewModel : INotifyPropertyChanged
    {
        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

        private ObservableCollection<Supplier> suppliers;
        public ObservableCollection<Supplier> Suppliers
        {
            get => suppliers;
            set
            {
                suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        private Supplier editableSupplier;
        public Supplier EditableSupplier
        {
            get => editableSupplier;
            set
            {
                editableSupplier = value;
                OnPropertyChanged(nameof(EditableSupplier));
                if (value != null)
                {
                    SupplierName = value.Name;
                    SupplierCountry = value.Country;
                }
            }
        }

        private Supplier selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => selectedSupplier;
            set
            {
                selectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));
                if (value != null)
                {
                    SupplierName = value.Name;
                    SupplierCountry = value.Country;
                }
            }
        }

        private string supplierName;
        public string SupplierName
        {
            get => supplierName;
            set
            {
                supplierName = value;
                OnPropertyChanged(nameof(SupplierName));
            }
        }

        private string supplierCountry;
        public string SupplierCountry
        {
            get => supplierCountry;
            set
            {
                supplierCountry = value;
                OnPropertyChanged(nameof(SupplierCountry));
            }
        }

        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand DeactivateSupplierCommand { get; }

        public SupplierViewModel()
        {
            Suppliers = new ObservableCollection<Supplier>();
            LoadSuppliers();

            AddSupplierCommand = new RelayCommand(AddSupplier);
            EditSupplierCommand = new RelayCommand(EditSupplier);
            DeactivateSupplierCommand = new RelayCommand(DeactivateSupplier);
        }

        private void LoadSuppliers()
        {
            Suppliers.Clear();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT ID_Producator, Nume_Producator, Tara_Origine FROM Producatori WHERE Activ = 1", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Suppliers.Add(new Supplier
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Country = reader.GetString(2)
                        });
                    }
                }
            }
        }

        private void AddSupplier()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("INSERT INTO Producatori (Nume_Producator, Tara_Origine, Activ) VALUES (@Name, @Country, 1)", connection);
                cmd.Parameters.AddWithValue("@Name", SupplierName);
                cmd.Parameters.AddWithValue("@Country", SupplierCountry);
                cmd.ExecuteNonQuery();
            }
            LoadSuppliers();
        }

        private void EditSupplier()
        {
            if (EditableSupplier != null)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand("UPDATE Producatori SET Nume_Producator = @Name, Tara_Origine = @Country WHERE ID_Producator = @Id", connection);
                    cmd.Parameters.AddWithValue("@Id", EditableSupplier.Id);
                    cmd.Parameters.AddWithValue("@Name", EditableSupplier.Name); // Ensure this is updated if user changes the name in UI
                    cmd.Parameters.AddWithValue("@Country", EditableSupplier.Country); // Ensure this is updated if user changes the country in UI
                    cmd.ExecuteNonQuery();
                }
                LoadSuppliers();
            }
        }

        private void DeactivateSupplier()
        {
            if (SelectedSupplier != null)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand("UPDATE Producatori SET Activ = 0 WHERE ID_Producator = @Id", connection);
                    cmd.Parameters.AddWithValue("@Id", SelectedSupplier.Id);
                    cmd.ExecuteNonQuery();
                }
                LoadSuppliers();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
