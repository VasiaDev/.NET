using System;

namespace DispensaryApp.Core.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Notes { get; set; }
        public decimal Cost { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
} 