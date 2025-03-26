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
    public class DoctorService : IDataService<Doctor>
    {
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(ILogger<DoctorService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Doctor> GetAllDoctors()
        {
            return GetAllAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Начало загрузки врачей");
                using var context = DispensaryDbContextFactory.CreateContext();
                var doctors = await context.Doctors.ToListAsync();
                _logger.LogInformation("Загружено {Count} врачей", doctors.Count);
                return doctors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке врачей");
                throw;
            }
        }

        public async Task<Doctor> GetByIdAsync(int id)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var result = await context.Doctors.FindAsync(id);
            if (result == null)
                throw new KeyNotFoundException($"Врач с ID {id} не найден");
            return result;
        }

        public async Task<Doctor> AddAsync(Doctor doctor)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var result = await context.Doctors.AddAsync(doctor);
            await context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Doctor> UpdateAsync(Doctor doctor)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var result = context.Doctors.Update(doctor);
            await context.SaveChangesAsync();
            return result.Entity;
        }

        public async Task DeleteAsync(int id)
        {
            using var context = DispensaryDbContextFactory.CreateContext();
            var doctor = await context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                context.Doctors.Remove(doctor);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Doctor> SaveDoctor(Doctor doctor)
        {
            if (doctor.Id == 0)
            {
                return await AddAsync(doctor);
            }
            else
            {
                return await UpdateAsync(doctor);
            }
        }
    }
} 