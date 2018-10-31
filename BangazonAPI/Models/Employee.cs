using System;
using System.Collections.Generic;

namespace BangazonAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int DepartmentId { get; set; }
        public bool IsSuperVisor { get; set; }
        public string Department { get; set; }
        public List<Computer> Computers { get; set; }
    }
}