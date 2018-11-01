using System.Collections.Generic;
using BangazonAPI;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer customer { get; set; }
        public int? PaymentTypeId { get; set; }
        public List<Product> productList = new List<Product>();
    }

}