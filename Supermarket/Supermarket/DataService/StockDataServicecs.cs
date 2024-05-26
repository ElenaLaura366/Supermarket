using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Supermarket.Model;

namespace Supermarket.DataService
{
    public class StockDataService
    {
        private readonly string _connectionString;

        public StockDataService()
        {
            _connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ID_Produs, Nume_Produs FROM Produse", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return products;
        }

        public List<Stock> GetAllStocks()
        {
            var stocks = new List<Stock>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT s.ID_Stoc, s.ID_Produs, p.Nume_Produs, s.Cantitate, s.Unitate_Masura, s.Data_Aprovizionare, 
                                          s.Data_Expirare, s.Pret_Achizitie, s.Pret_Vanzare, s.IsActive 
                                   FROM Stocuri s
                                   JOIN Produse p ON s.ID_Produs = p.ID_Produs
                                   WHERE s.IsActive = 1", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stocks.Add(new Stock
                        {
                            ID = reader.GetInt32(0),
                            ID_Produs = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            Cantitate = reader.GetDecimal(3),
                            Unitate_Masura = reader.GetString(4),
                            Data_Aprovizionare = reader.GetDateTime(5),
                            Data_Expirare = reader.GetDateTime(6),
                            Pret_Achizitie = reader.GetDecimal(7),
                            Pret_Vanzare = reader.GetDecimal(8),
                            IsActive = reader.GetBoolean(9)
                        });
                    }
                }
            }
            return stocks;
        }

        public void AddStock(Stock stock)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Stocuri (ID_Produs, Cantitate, Unitate_Masura, Data_Aprovizionare, Data_Expirare, Pret_Achizitie, Pret_Vanzare, IsActive) VALUES (@ID_Produs, @Cantitate, @Unitate_Masura, @Data_Aprovizionare, @Data_Expirare, @Pret_Achizitie, @Pret_Vanzare, @IsActive)", conn);
                cmd.Parameters.AddWithValue("@ID_Produs", stock.ID_Produs);
                cmd.Parameters.AddWithValue("@Cantitate", stock.Cantitate);
                cmd.Parameters.AddWithValue("@Unitate_Masura", string.IsNullOrEmpty(stock.Unitate_Masura) ? DBNull.Value : (object)stock.Unitate_Masura);
                cmd.Parameters.AddWithValue("@Data_Aprovizionare", stock.Data_Aprovizionare);
                cmd.Parameters.AddWithValue("@Data_Expirare", stock.Data_Expirare);
                cmd.Parameters.AddWithValue("@Pret_Achizitie", stock.Pret_Achizitie);
                cmd.Parameters.AddWithValue("@Pret_Vanzare", stock.Pret_Vanzare);
                cmd.Parameters.AddWithValue("@IsActive", stock.IsActive);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateStockPrice(int stockId, decimal newPrice)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Stocuri SET Pret_Vanzare = @Pret_Vanzare WHERE ID_Stoc = @ID_Stoc", conn);
                cmd.Parameters.AddWithValue("@ID_Stoc", stockId);
                cmd.Parameters.AddWithValue("@Pret_Vanzare", newPrice);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteStock(Stock stock)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Stocuri SET IsActive = 0 WHERE ID_Stoc = @ID_Stoc", conn);
                cmd.Parameters.AddWithValue("@ID_Stoc", stock.ID);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
