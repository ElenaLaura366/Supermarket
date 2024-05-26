using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Input;
using Supermarket.Command;
using Supermarket.Model;

namespace Supermarket.ViewModels
{
    public class ReceiptSellerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ProdusBon> produseAdaugate;
        private ObservableCollection<Product> produse;
        private Product produsSelectat;
        private decimal cantitate;

        public ObservableCollection<ProdusBon> ProduseAdaugate
        {
            get { return produseAdaugate; }
            set
            {
                produseAdaugate = value;
                OnPropertyChanged(nameof(ProduseAdaugate));
            }
        }

        public ObservableCollection<Product> Produse
        {
            get { return produse; }
            set
            {
                produse = value;
                OnPropertyChanged(nameof(Produse));
            }
        }

        public Product ProdusSelectat
        {
            get { return produsSelectat; }
            set
            {
                produsSelectat = value;
                OnPropertyChanged(nameof(ProdusSelectat));
            }
        }

        public decimal Cantitate
        {
            get { return cantitate; }
            set
            {
                cantitate = value;
                OnPropertyChanged(nameof(Cantitate));
            }
        }

        public ICommand AddCommand { get; }
        public ICommand OkCommand { get; }

        public ReceiptSellerViewModel()
        {
            ProduseAdaugate = new ObservableCollection<ProdusBon>();
            Produse = new ObservableCollection<Product>();
            AddCommand = new RelayCommand(AddProdus);
            OkCommand = new RelayCommand(FinalizeazaBon);

            LoadProduse();
        }

        private void LoadProduse()
        {
            string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID_Produs, Nume_Produs, ID_Categorie, ID_Producator, Activ FROM Produse";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Produse.Add(new Product
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            CategoryId = reader.GetInt32(2),
                            ProducerId = reader.GetInt32(3),
                            IsActive = reader.GetBoolean(4)
                        });
                    }
                }
            }
        }

        private void AddProdus()
        {
            if (ProdusSelectat == null || Cantitate <= 0)
            {
                // Logica pentru a gestiona erorile de intrare
                Console.WriteLine("Produsul nu a fost selectat sau cantitatea este invalidă.");
                return;
            }

            // Obținem prețul produsului din stocuri
            decimal pretVanzare = 0;
            string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT TOP 1 Pret_Vanzare FROM Stocuri WHERE ID_Produs = @IDProdus AND IsActive = 1 ORDER BY Data_Aprovizionare DESC";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IDProdus", ProdusSelectat.ID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        pretVanzare = reader.GetDecimal(0);
                    }
                    else
                    {
                        Console.WriteLine("Nu există stoc activ pentru produsul selectat.");
                        return;
                    }
                }
            }

            ProduseAdaugate.Add(new ProdusBon
            {
                IDProdus = ProdusSelectat.ID,
                NumeProdus = ProdusSelectat.Name,
                Cantitate = Cantitate,
                Pret = pretVanzare,
                Subtotal = Cantitate * pretVanzare
            });

            // Resetăm cantitatea după adăugare
            Cantitate = 0;
            ProdusSelectat = null;

            OnPropertyChanged(nameof(ProduseAdaugate));
        }

        private void FinalizeazaBon()
        {
            int idCasier = 1; // Id-ul casierului curent
            DateTime dataEliberare = DateTime.Now;
            decimal sumaIncasata = ProduseAdaugate.Sum(p => p.Subtotal);

            string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    string insertBonQuery = "INSERT INTO Bonuri_de_Casa (Data_Eliberare, ID_Casier, Suma_Incasata) OUTPUT INSERTED.ID_Bon VALUES (@DataEliberare, @IDCasier, @SumaIncasata)";
                    SqlCommand insertBonCommand = new SqlCommand(insertBonQuery, connection, transaction);
                    insertBonCommand.Parameters.AddWithValue("@DataEliberare", dataEliberare);
                    insertBonCommand.Parameters.AddWithValue("@IDCasier", idCasier);
                    insertBonCommand.Parameters.AddWithValue("@SumaIncasata", sumaIncasata);

                    int idBon = (int)insertBonCommand.ExecuteScalar();

                    string insertDetaliiBonQuery = "INSERT INTO Detalii_Bon (ID_Bon, ID_Produs, Cantitate, Subtotal) VALUES (@IDBon, @IDProdus, @Cantitate, @Subtotal)";
                    foreach (var produs in ProduseAdaugate)
                    {
                        SqlCommand insertDetaliiBonCommand = new SqlCommand(insertDetaliiBonQuery, connection, transaction);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@IDBon", idBon);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@IDProdus", produs.IDProdus);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@Cantitate", produs.Cantitate);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@Subtotal", produs.Subtotal);

                        insertDetaliiBonCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    ProduseAdaugate.Clear();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // Logica de gestionare a erorilor
                    Console.WriteLine($"A apărut o eroare: {ex.Message}");
                }
            }

            OnPropertyChanged(nameof(ProduseAdaugate));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProdusBon
    {
        public int IDProdus { get; set; }
        public string NumeProdus { get; set; }
        public decimal Cantitate { get; set; }
        public decimal Pret { get; set; }
        public decimal Subtotal { get; set; }
    }
}
