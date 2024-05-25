using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supermarket
{
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool isAdmin { get; set; }
    }
    public class UsersList
    {
        public List<User> users { get; set; }
    }
}
