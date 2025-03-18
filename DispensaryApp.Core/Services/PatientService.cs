using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.Core.Models;
using DispensaryApp.Data;

namespace DispensaryApp.Core.Services
{
    public class PatientService : IDataService<Patient>
    {
        private readonly DispensaryDbContext _context;

        public DispensaryDbContext Context => _context;

        public PatientService(DispensaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Patient> GetAllPatients()
        {
            return GetAllAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .Select(p => new Patient
                {
                    Id = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    MiddleName = "",
                    InsurancePolicy = "",
                    Phone = "",
                    Email = "",
                    Address = "",
                    BirthDate = p.DateOfBirth
                })
                .ToListAsync();
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            var dbPatient = await _context.Patients.FindAsync(id);
            if (dbPatient == null)
                return null;

            return new Patient
            {
                Id = dbPatient.PatientId,
                FirstName = dbPatient.FirstName,
                LastName = dbPatient.LastName,
                MiddleName = "",
                InsurancePolicy = "",
                Phone = "",
                Email = "",
                Address = "",
                BirthDate = dbPatient.DateOfBirth
            };
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            var dbPatient = new Data.Models.Patient
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.BirthDate,
                Gender = "Не указан"
            };

            _context.Patients.Add(dbPatient);
            await _context.SaveChangesAsync();

            patient.Id = dbPatient.PatientId;
            return patient;
        }

        public async Task<Patient> UpdateAsync(Patient patient)
        {
            var dbPatient = await _context.Patients.FindAsync(patient.Id);
            if (dbPatient == null)
                throw new KeyNotFoundException($"Пациент с ID {patient.Id} не найден");

            dbPatient.FirstName = patient.FirstName;
            dbPatient.LastName = patient.LastName;
            dbPatient.DateOfBirth = patient.BirthDate;

            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
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