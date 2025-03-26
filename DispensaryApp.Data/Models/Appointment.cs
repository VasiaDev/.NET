using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispensaryApp.Data.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        Cancelled
    }

    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentId { get; set; }

        // Свойство Id для совместимости с интерфейсом
        [NotMapped]
        public int Id 
        { 
            get => AppointmentId;
            set => AppointmentId = value;
        }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        // Свойство Date для совместимости с существующим кодом
        [NotMapped]
        public DateTime Date
        {
            get => AppointmentDate;
            set => AppointmentDate = value;
        }

        // Свойство Time для совместимости с существующим кодом
        [NotMapped]
        public TimeSpan Time
        {
            get => AppointmentDate.TimeOfDay;
            set => AppointmentDate = AppointmentDate.Date + value;
        }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
} 