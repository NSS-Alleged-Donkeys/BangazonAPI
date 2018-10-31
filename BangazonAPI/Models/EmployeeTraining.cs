using System;
using System.Collections.Generic;

namespace BangazonAPI.Models
{
    public class EmployeeTraining
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int TrainingProgramId { get; set; }
    }
}
