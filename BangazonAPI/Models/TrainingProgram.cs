using System;
using System.Collections.Generic;

namespace BangazonAPI.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxAttendees { get; set; }
        public List<Employee> EmployeeList { get; set; } = new List<Employee>();
    }
}
