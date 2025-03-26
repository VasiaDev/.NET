using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.Data;
using DispensaryApp.Data.Models;
using Microsoft.Extensions.Logging;

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
            using var context = DispensaryDbContextFactory.CreateContext();
            var result = await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var result = context.Appointments.Update(appointment);
            await context.SaveChangesAsync();
            return result.Entity;
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
    }
} 