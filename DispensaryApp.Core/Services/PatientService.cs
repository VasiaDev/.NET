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
    public class PatientService : IDataService<Patient>
    {
        private readonly ILogger<PatientService> _logger;

        public PatientService(ILogger<PatientService> logger)
        {
            _logger = logger;
            _logger.LogInformation("PatientService создан");
        }

        public IEnumerable<Patient> GetAllPatients()
        {
            return GetAllAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Начало загрузки пациентов");
                using var context = DispensaryDbContextFactory.CreateContext();
                _logger.LogInformation("Контекст базы данных создан");

                var patients = await context.Patients.ToListAsync();
                if (patients == null)
                {
                    _logger.LogWarning("Список пациентов равен null");
                    return new List<Patient>();
                }

                _logger.LogInformation("Загружено {Count} пациентов", patients.Count);
                foreach (var patient in patients)
                {
                    _logger.LogDebug("Загружен пациент: ID={Id}, ФИО={LastName} {FirstName} {MiddleName}",
                        patient.Id, patient.LastName, patient.FirstName, patient.MiddleName);
                }
                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке пациентов: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Поиск пациента по ID={Id}", id);
                using var context = DispensaryDbContextFactory.CreateContext();
                var result = await context.Patients.FindAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Пациент с ID={Id} не найден", id);
                    throw new KeyNotFoundException($"Пациент с ID {id} не найден");
                }
                _logger.LogInformation("Найден пациент: ID={Id}, ФИО={LastName} {FirstName} {MiddleName}",
                    result.Id, result.LastName, result.FirstName, result.MiddleName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске пациента по ID={Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            try
            {
                _logger.LogInformation("Добавление нового пациента: ФИО={LastName} {FirstName} {MiddleName}",
                    patient.LastName, patient.FirstName, patient.MiddleName);
                using var context = DispensaryDbContextFactory.CreateContext();
                var result = await context.Patients.AddAsync(patient);
                await context.SaveChangesAsync();
                _logger.LogInformation("Пациент успешно добавлен с ID={Id}", result.Entity.Id);
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пациента: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Patient> UpdateAsync(Patient patient)
        {
            try
            {
                _logger.LogInformation("Обновление пациента: ID={Id}, ФИО={LastName} {FirstName} {MiddleName}",
                    patient.Id, patient.LastName, patient.FirstName, patient.MiddleName);
                using var context = DispensaryDbContextFactory.CreateContext();
                var result = context.Patients.Update(patient);
                await context.SaveChangesAsync();
                _logger.LogInformation("Пациент успешно обновлен");
                return result.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении пациента: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Удаление пациента с ID={Id}", id);
                using var context = DispensaryDbContextFactory.CreateContext();
                var patient = await context.Patients.FindAsync(id);
                if (patient != null)
                {
                    context.Patients.Remove(patient);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Пациент успешно удален");
                }
                else
                {
                    _logger.LogWarning("Пациент с ID={Id} не найден для удаления", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пациента: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Patient> SavePatient(Patient patient)
        {
            if (patient.Id == 0)
            {
                return await AddAsync(patient);
            }
            else
            {
                return await UpdateAsync(patient);
            }
        }
    }
} 