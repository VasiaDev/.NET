using System;

namespace DispensaryApp.Core.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Specialization { get; set; }
        public string LicenseNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Schedule { get; set; }
        public DateTime HireDate { get; set; }
    }
} 