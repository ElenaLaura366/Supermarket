using Supermarket.Command;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Input;
using Supermarket.Model;
using System.Collections.Generic;

public class UserViewModel : INotifyPropertyChanged
{
    private ObservableCollection<User> _users;
    public ObservableCollection<User> Users
    {
        get { return _users; }
        set { _users = value; OnPropertyChanged(nameof(Users)); }
    }

    private User _selectedUser;
    public User SelectedUser
    {
        get { return _selectedUser; }
        set { _selectedUser = value; OnPropertyChanged(nameof(SelectedUser)); }
    }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand LoadUsersCommand { get; }

    public UserViewModel()
    {
        AddUserCommand = new RelayCommand(AddUser);
        EditUserCommand = new RelayCommand(EditUser);
        DeleteUserCommand = new RelayCommand(DeleteUser);
        LoadUsersCommand = new RelayCommand(LoadUsers);
        Users = new ObservableCollection<User>();
        LoadUsers(null);
    }

    private readonly string connectionString = "Server=DESKTOP-O046ND7;Database=Supermarket;Trusted_Connection=True;TrustServerCertificate=True;";

    private string _newUsername;
    private string _newPassword;

    public string NewUsername
    {
        get { return _newUsername; }
        set { _newUsername = value; OnPropertyChanged(nameof(NewUsername)); }
    }

    public string NewPassword
    {
        get { return _newPassword; }
        set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); }
    }

    public List<string> UserTypes { get; set; } = new List<string> { "Administrator", "Casier" };

    private string _newUserType = "Casier";
    public string NewUserType
    {
        get { return _newUserType; }
        set { _newUserType = value; OnPropertyChanged(nameof(NewUserType)); }
    }

    private void AddUser(object parameter)
    {
        using (var conn = new SqlConnection(connectionString))
        {
            var cmd = new SqlCommand("INSERT INTO Utilizatori (Nume_Utilizator, Parola, Tip_Utilizator, IsActive) VALUES (@Username, @Password, @UserType, 1)", conn);
            cmd.Parameters.AddWithValue("@Username", NewUsername);
            cmd.Parameters.AddWithValue("@Password", NewPassword);
            cmd.Parameters.AddWithValue("@UserType", NewUserType); // Make sure this aligns with your database schema
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        LoadUsers(null);
    }
    private void EditUser(object parameter)
    {
        if (SelectedUser != null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand("UPDATE Utilizatori SET Nume_Utilizator = @Username, Parola = @Password, Tip_Utilizator = @UserType WHERE ID_Utilizator = @ID_Utilizator", conn);
                cmd.Parameters.AddWithValue("@ID_Utilizator", SelectedUser.ID);
                cmd.Parameters.AddWithValue("@Username", SelectedUser.username);
                cmd.Parameters.AddWithValue("@Password", SelectedUser.password);
                cmd.Parameters.AddWithValue("@UserType", SelectedUser.userType);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            LoadUsers(null);
        }
    }

    private void DeleteUser(object parameter)
    {
        if (SelectedUser != null)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand("UPDATE Utilizatori SET IsActive = 0 WHERE ID_Utilizator = @ID_Utilizator", conn);
                cmd.Parameters.AddWithValue("@ID_Utilizator", SelectedUser.ID);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            LoadUsers(null);
        }
    }

    private void LoadUsers(object parameter)
    {
        using (var conn = new SqlConnection(connectionString))
        {
            var cmd = new SqlCommand("SELECT ID_Utilizator, Nume_Utilizator, Parola, Tip_Utilizator, IsActive FROM Utilizatori WHERE IsActive = 1", conn);
            var users = new ObservableCollection<User>();
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        ID = reader.GetInt32(0),
                        username = reader.GetString(1),
                        password = reader.GetString(2),
                        userType = reader.GetString(3),
                        isActive = reader.GetBoolean(4)
                    });
                }
            }
            conn.Close();
            Users = users;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
