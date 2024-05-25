using System;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using Supermarket.Command;
using System.Data.SqlClient;
using Supermarket.Views;

namespace Supermarket.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public ICommand LoginCommand { get; }
        private string username;
        private string password;
        private bool isUserAdmin;

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        public bool IsUserAdmin
        {
            get => isUserAdmin;
            set
            {
                if (value != isUserAdmin)
                {
                    isUserAdmin = value;
                    OnPropertyChanged(nameof(IsUserAdmin));
                }
            }
        }

        private string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
        }

        private void Login()
        {
            if (TestDatabaseConnection())
            {
                var user = AuthenticateUser(Username, Password);
                if (user != null)
                {
                    MessageBox.Show("Login successful!");
                    IsUserAdmin = user.isAdmin;
                    ShowWindow();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
            }
            else
            {
                MessageBox.Show("Database connection failed.");
            }
        }

        private void ShowWindow()
        {
            if(isUserAdmin)
            {
                var adminWindow = new AdministratorWindow();
                adminWindow.DataContext = new AdministratorViewModel();
                adminWindow.Show();
            }
            else
            {
                var casierWindow = new CasierWindow();
                casierWindow.DataContext = new CasierViewModel();
                casierWindow.Show();
            }
            CloseLoginWindow();
        }

        private void CloseLoginWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                }
            }
        }

        private bool TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private User AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Utilizatori WHERE Nume_Utilizator = @Nume_Utilizator AND Parola = @Parola";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nume_Utilizator", username);
                        command.Parameters.AddWithValue("@Parola", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    username = reader["Nume_Utilizator"].ToString(),
                                    password = reader["Parola"].ToString(),
                                    isAdmin = reader["Tip_Utilizator"].ToString() == "Administrator"
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
