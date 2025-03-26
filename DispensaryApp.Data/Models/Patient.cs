using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DispensaryApp.Data.Models
{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientId { get; set; }

        // Свойство Id для совместимости с интерфейсом
        [NotMapped]
        public int Id 
        { 
            get => PatientId;
            set => PatientId = value;
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
        public DateTime DateOfBirth { get; set; }

        // Свойство BirthDate для совместимости с существующим кодом
        [NotMapped]
        public DateTime BirthDate
        {
            get => DateOfBirth;
            set => DateOfBirth = value;
        }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = "";

        [StringLength(50)]
        public string? InsurancePolicy { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 