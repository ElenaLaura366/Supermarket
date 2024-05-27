using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace Supermarket.ViewModels
{
    public class ReceiptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DateTime selectedDate = DateTime.Today;
        private decimal maxReceiptTotal;
        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                if (selectedDate != value)
                {
                    selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    LoadMaxReceiptTotal();
                }
            }
        }

        public decimal MaxReceiptTotal
        {
            get { return maxReceiptTotal; }
            set
            {
                maxReceiptTotal = value;
                OnPropertyChanged(nameof(MaxReceiptTotal));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadMaxReceiptTotal()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT MAX(Suma_Incasata) AS MaxTotal
                    FROM Bonuri_de_Casa
                    WHERE CAST(Data_Eliberare AS DATE) = @SelectedDate";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SelectedDate", SelectedDate);

                var result = command.ExecuteScalar();
                MaxReceiptTotal = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        public ReceiptViewModel()
        {
            LoadMaxReceiptTotal(); // Load initial data
        }
    }
}
