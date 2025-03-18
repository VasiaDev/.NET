using System;

namespace DispensaryApp.Core.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public required string MiddleName { get; set; }
        public required string Specialization { get; set; }
        public required string LicenseNumber { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Schedule { get; set; }
        public DateTime HireDate { get; set; }
    }
} 