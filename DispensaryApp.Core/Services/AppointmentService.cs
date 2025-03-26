using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.Data;
using DispensaryApp.Data.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DispensaryApp.Core.Services
{
    public class AppointmentService : IDataService<Appointment>
    {
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(ILogger<AppointmentService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Appointment> GetAllAppointments()
        {
            return GetAllAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Начало загрузки приемов");
                using var context = DispensaryDbContextFactory.CreateContext();
                var appointments = await context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .ToListAsync();
                _logger.LogInformation("Загружено {Count} приемов", appointments.Count);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке приемов");
                throw;
            }
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var result = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (result == null)
                throw new KeyNotFoundException($"Прием с ID {id} не найден");

            return result;
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            ValidateAppointment(appointment);

            using var context = DispensaryDbContextFactory.CreateContext();
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            ValidateAppointment(appointment);

            using var context = DispensaryDbContextFactory.CreateContext();
            context.Entry(appointment).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return appointment;
        }

        public async Task DeleteAsync(int id)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var appointment = await context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                context.Appointments.Remove(appointment);
                await context.SaveChangesAsync();
            }
        }

        public async Task CancelAppointmentAsync(int id)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var appointment = await context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                await context.SaveChangesAsync();
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

        private void ValidateAppointment(Appointment appointment)
        {
            var validationContext = new ValidationContext(appointment);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(appointment, validationContext, validationResults, true))
            {
                var errors = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }

            if (appointment.AppointmentDate < DateTime.Now)
            {
                throw new ValidationException("Дата приема не может быть в прошлом");
            }

            using var context = DispensaryDbContextFactory.CreateContext();
            var existingAppointments = context.Appointments
                .Where(a => a.DoctorId == appointment.DoctorId && 
                           a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                           a.Id != appointment.Id)
                .ToList();

            foreach (var existing in existingAppointments)
            {
                if (Math.Abs((existing.AppointmentDate - appointment.AppointmentDate).TotalMinutes) < 30)
                {
                    throw new ValidationException("У врача уже есть прием в это время");
                }
            }
        }
    }
} 