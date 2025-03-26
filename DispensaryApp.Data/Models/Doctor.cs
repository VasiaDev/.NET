using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispensaryApp.Data.Models
{
    public class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DoctorId { get; set; }

        // Свойство Id для совместимости с интерфейсом
        [NotMapped]
        public int Id 
        { 
            get => DoctorId;
            set => DoctorId = value;
        }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; } = "";

        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(20)]
        public string? LicenseNumber { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Schedule { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 