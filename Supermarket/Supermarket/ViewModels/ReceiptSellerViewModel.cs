using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
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
        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";
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
        private bool VerificaSiActualizeazaStoc(int idProdus, decimal cantitateDorita)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    string queryStoc = @"
                        SELECT Cantitate, Data_Expirare 
                        FROM Stocuri 
                        WHERE ID_Produs = @IDProdus AND IsActive = 1 AND Data_Expirare >= CAST(GETDATE() AS DATE)";

                    SqlCommand commandStoc = new SqlCommand(queryStoc, connection, transaction);
                    commandStoc.Parameters.AddWithValue("@IDProdus", idProdus);

                    using (SqlDataReader reader = commandStoc.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Console.WriteLine("Stocul este inactiv sau a expirat pentru produsul selectat.");
                            return false;
                        }

                        decimal cantitateInStoc = reader.GetDecimal(0);
                        if (cantitateDorita > cantitateInStoc)
                        {
                            Console.WriteLine("Stoc insuficient pentru produsul selectat.");
                            return false;
                        }
                    }

                    string updateStoc = @"
                        UPDATE Stocuri 
                        SET Cantitate = Cantitate - @CantitateDorita 
                        WHERE ID_Produs = @IDProdus AND IsActive = 1";

                    SqlCommand updateCommand = new SqlCommand(updateStoc, connection, transaction);
                    updateCommand.Parameters.AddWithValue("@CantitateDorita", cantitateDorita);
                    updateCommand.Parameters.AddWithValue("@IDProdus", idProdus);

                    updateCommand.ExecuteNonQuery();

                    // Deactivate stock if quantity is zero
                    string deactivateStock = @"
                        UPDATE Stocuri 
                        SET IsActive = 0 
                        WHERE ID_Produs = @IDProdus AND Cantitate <= 0";

                    SqlCommand deactivateCommand = new SqlCommand(deactivateStock, connection, transaction);
                    deactivateCommand.Parameters.AddWithValue("@IDProdus", idProdus);
                    deactivateCommand.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Eroare la actualizarea stocului: {ex.Message}");
                    return false;
                }
            }
        }

        private void AddProdus()
        {
            if (ProdusSelectat == null || Cantitate <= 0)
            {
                Console.WriteLine("Produsul nu a fost selectat sau cantitatea este invalidă.");
                return;
            }

            if (!VerificaSiActualizeazaStoc(ProdusSelectat.ID, Cantitate))
            {
                return;
            }

            decimal pretVanzare = 0;
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

            Cantitate = 0;
            ProdusSelectat = null;

            OnPropertyChanged(nameof(ProduseAdaugate));
        }
        private void FinalizeazaBon()
        {
            if (!ProduseAdaugate.Any())
            {
                MessageBox.Show("Nu există produse adăugate pe bon pentru a finaliza.");
                return;
            }

            int idCasier = 1;
            DateTime dataEliberare = DateTime.Now;
            decimal sumaIncasata = ProduseAdaugate.Sum(p => p.Subtotal);

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

                    foreach (var produs in ProduseAdaugate)
                    {
                        string insertDetaliiBonQuery = "INSERT INTO Detalii_Bon (ID_Bon, ID_Produs, Cantitate, Subtotal) VALUES (@IDBon, @IDProdus, @Cantitate, @Subtotal)";
                        SqlCommand insertDetaliiBonCommand = new SqlCommand(insertDetaliiBonQuery, connection, transaction);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@IDBon", idBon);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@IDProdus", produs.IDProdus);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@Cantitate", produs.Cantitate);
                        insertDetaliiBonCommand.Parameters.AddWithValue("@Subtotal", produs.Subtotal);

                        insertDetaliiBonCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    ProduseAdaugate.Clear();

                    // Afișăm un MessageBox cu suma totală încasată
                    MessageBox.Show($"Bonul a fost finalizat cu succes. Suma totală încasată: {sumaIncasata:C2}", "Finalizare Bon", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"A apărut o eroare la finalizarea bonului: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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
