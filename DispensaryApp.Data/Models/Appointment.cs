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
        public int Id { get; set; }

        [Required(ErrorMessage = "Пациент не выбран")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Врач не выбран")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Дата приема не указана")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [NotMapped]
        public DateTime Date
        {
            get => AppointmentDate;
            set => AppointmentDate = value;
        }

        [NotMapped]
        public TimeSpan Time
        {
            get => AppointmentDate.TimeOfDay;
            set => AppointmentDate = AppointmentDate.Date + value;
        }

        [Required(ErrorMessage = "Причина приема не указана")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Причина приема должна содержать от 10 до 500 символов")]
        public string Reason { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Статус приема не указан")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
} 