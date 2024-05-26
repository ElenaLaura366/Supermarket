using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supermarket.Model
{
    public class User
    {
        public int ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string userType { get; set; }
        public bool isActive { get; set; } = true;
        public bool isAdmin { get; set; }
    }

    public class UsersList
    {
        public List<User> users { get; set; }
    }
}
