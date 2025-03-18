using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.Core.Models;
using DispensaryApp.Data;

namespace DispensaryApp.Core.Services
{
    public class AppointmentService : IDataService<Appointment>
    {
        private readonly DispensaryDbContext _context;

        public DispensaryDbContext Context => _context;

        public AppointmentService(DispensaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Appointment> GetAllAppointments()
        {
            return GetAllAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Select(a => new Appointment
                {
                    Id = a.AppointmentId,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    Date = a.AppointmentDate.Date,
                    Time = a.AppointmentDate.TimeOfDay,
                    Status = "Запланирован",
                    Type = "Первичный",
                    Notes = a.Reason,
                    Cost = 0,
                    IsPaid = false,
                    CreatedAt = a.CreatedAt,
                    Patient = a.Patient != null ? new Patient
                    {
                        Id = a.Patient.PatientId,
                        FirstName = a.Patient.FirstName,
                        LastName = a.Patient.LastName,
                        MiddleName = "Не указано",
                        InsurancePolicy = "Не указано",
                        Phone = "Не указано",
                        Email = "Не указано",
                        Address = "Не указано",
                        BirthDate = a.Patient.DateOfBirth
                    } : null,
                    Doctor = a.Doctor != null ? new Doctor
                    {
                        Id = a.Doctor.DoctorId,
                        FirstName = a.Doctor.FirstName,
                        LastName = a.Doctor.LastName,
                        MiddleName = "Не указано",
                        Specialization = a.Doctor.Specialty,
                        LicenseNumber = "Не указано",
                        Phone = "Не указано",
                        Email = "Не указано",
                        Schedule = "Не указано",
                        HireDate = DateTime.Now
                    } : null
                })
                .ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            var dbAppointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (dbAppointment == null)
                return null;

            return new Appointment
            {
                Id = dbAppointment.AppointmentId,
                PatientId = dbAppointment.PatientId,
                DoctorId = dbAppointment.DoctorId,
                Date = dbAppointment.AppointmentDate.Date,
                Time = dbAppointment.AppointmentDate.TimeOfDay,
                Status = "Запланирован",
                Type = "Первичный",
                Notes = dbAppointment.Reason,
                Cost = 0,
                IsPaid = false,
                CreatedAt = dbAppointment.CreatedAt,
                Patient = dbAppointment.Patient != null ? new Patient
                {
                    Id = dbAppointment.Patient.PatientId,
                    FirstName = dbAppointment.Patient.FirstName,
                    LastName = dbAppointment.Patient.LastName,
                    MiddleName = "Не указано",
                    InsurancePolicy = "Не указано",
                    Phone = "Не указано",
                    Email = "Не указано",
                    Address = "Не указано",
                    BirthDate = dbAppointment.Patient.DateOfBirth
                } : null,
                Doctor = dbAppointment.Doctor != null ? new Doctor
                {
                    Id = dbAppointment.Doctor.DoctorId,
                    FirstName = dbAppointment.Doctor.FirstName,
                    LastName = dbAppointment.Doctor.LastName,
                    MiddleName = "Не указано",
                    Specialization = dbAppointment.Doctor.Specialty,
                    LicenseNumber = "Не указано",
                    Phone = "Не указано",
                    Email = "Не указано",
                    Schedule = "Не указано",
                    HireDate = DateTime.Now
                } : null
            };
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            var dbAppointment = new Data.Models.Appointment
            {
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                AppointmentDate = appointment.Date.Add(appointment.Time),
                Reason = appointment.Notes ?? "Не указана"
            };

            _context.Appointments.Add(dbAppointment);
            await _context.SaveChangesAsync();

            appointment.Id = dbAppointment.AppointmentId;
            return appointment;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            var dbAppointment = await _context.Appointments.FindAsync(appointment.Id);
            if (dbAppointment == null)
                throw new KeyNotFoundException($"Прием с ID {appointment.Id} не найден");

            dbAppointment.PatientId = appointment.PatientId;
            dbAppointment.DoctorId = appointment.DoctorId;
            dbAppointment.AppointmentDate = appointment.Date.Add(appointment.Time);
            dbAppointment.Reason = appointment.Notes ?? "Не указана";

            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Reason = "Отменен: " + appointment.Reason;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Appointment> SaveAppointment(Appointment appointment)
        {
            if (appointment.Id == 0)
            {
                return await AddAsync(appointment);
            }
            else
            {
                return await UpdateAsync(appointment);
            }
        }
    }
} 