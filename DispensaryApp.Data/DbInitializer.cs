using System;
using System.Linq;
using DispensaryApp.Data.Models;

namespace DispensaryApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DispensaryDbContext context)
        {
            // Проверяем, есть ли уже данные
            if (context.Doctors.Any() || context.Patients.Any() || context.Appointments.Any())
            {
                return; // База данных уже заполнена
            }

            // Добавляем докторов
            var doctors = new Doctor[]
            {
                new Doctor
                {
                    FirstName = "Иван",
                    LastName = "Петров",
                    MiddleName = "Сергеевич",
                    Specialty = "Терапевт",
                    LicenseNumber = "ЛО-001-123456"
                },
                new Doctor
                {
                    FirstName = "Елена",
                    LastName = "Иванова",
                    MiddleName = "Александровна",
                    Specialty = "Кардиолог",
                    LicenseNumber = "ЛО-001-654321"
                }
            };

            context.Doctors.AddRange(doctors);
            context.SaveChanges();

            // Добавляем пациентов
            var patients = new Patient[]
            {
                new Patient
                {
                    FirstName = "Анна",
                    LastName = "Сидорова",
                    MiddleName = "Ивановна",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    Gender = "Женский",
                    Phone = "+7(999)123-45-67",
                    Address = "ул. Ленина, д. 10, кв. 5"
                },
                new Patient
                {
                    FirstName = "Петр",
                    LastName = "Васильев",
                    MiddleName = "Николаевич",
                    DateOfBirth = new DateTime(1985, 8, 20),
                    Gender = "Мужской",
                    Phone = "+7(999)765-43-21",
                    Address = "ул. Пушкина, д. 15, кв. 12"
                }
            };

            context.Patients.AddRange(patients);
            context.SaveChanges();

            // Добавляем приемы
            var appointments = new Appointment[]
            {
                new Appointment
                {
                    PatientId = patients[0].Id,
                    DoctorId = doctors[0].Id,
                    AppointmentDate = DateTime.Now.Date.AddDays(1).AddHours(10),
                    Reason = "Плановый осмотр",
                    Status = AppointmentStatus.Scheduled
                },
                new Appointment
                {
                    PatientId = patients[1].Id,
                    DoctorId = doctors[1].Id,
                    AppointmentDate = DateTime.Now.Date.AddDays(2).AddHours(14).AddMinutes(30),
                    Reason = "Консультация кардиолога",
                    Status = AppointmentStatus.Scheduled
                }
            };

            context.Appointments.AddRange(appointments);
            context.SaveChanges();
        }
    }
} 