using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MerchantId { get; set;  }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductTypeId { get; set;  }
    }
}