using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supermarket.Model
{
    public class Stock
    {
        public int ID { get; set; }
        public int ID_Produs { get; set; }
        public string ProductName { get; set; }
        public decimal Cantitate { get; set; }
        public string Unitate_Masura { get; set; }
        public DateTime Data_Aprovizionare { get; set; }
        public DateTime Data_Expirare { get; set; }
        public decimal Pret_Achizitie { get; set; }
        public decimal Pret_Vanzare { get; set; }
        public bool IsActive { get; set; }
    }
}
