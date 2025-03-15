using System;

namespace DispensaryApp.Core.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public required string MiddleName { get; set; }
        public required string InsurancePolicy { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public DateTime BirthDate { get; set; }
    }
} 