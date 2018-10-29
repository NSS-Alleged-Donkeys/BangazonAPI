using System;
using System.Collections.Generic;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string PaymentTypeId { get; set; }

    }
}