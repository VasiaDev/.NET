using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.Core.Models;
using DispensaryApp.Data;

namespace DispensaryApp.Core.Services
{
    public class DoctorService : IDataService<Doctor>
    {
        private readonly DispensaryDbContext _context;

        public DispensaryDbContext Context => _context;

        public DoctorService(DispensaryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Doctor> GetAllDoctors()
        {
            return GetAllAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _context.Doctors
                .Select(d => new Doctor
                {
                    Id = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    MiddleName = "",
                    Specialization = d.Specialty,
                    LicenseNumber = "",
                    Phone = "",
                    Email = "",
                    Schedule = "",
                    HireDate = DateTime.Now
                })
                .ToListAsync();
        }

        public async Task<Doctor> GetByIdAsync(int id)
        {
            var dbDoctor = await _context.Doctors.FindAsync(id);
            if (dbDoctor == null)
                return null;

            return new Doctor
            {
                Id = dbDoctor.DoctorId,
                FirstName = dbDoctor.FirstName,
                LastName = dbDoctor.LastName,
                MiddleName = "",
                Specialization = dbDoctor.Specialty,
                LicenseNumber = "",
                Phone = "",
                Email = "",
                Schedule = "",
                HireDate = DateTime.Now
            };
        }

        public async Task<Doctor> AddAsync(Doctor doctor)
        {
            var dbDoctor = new Data.Models.Doctor
            {
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialty = doctor.Specialization
            };

            _context.Doctors.Add(dbDoctor);
            await _context.SaveChangesAsync();

            doctor.Id = dbDoctor.DoctorId;
            return doctor;
        }

        public async Task<Doctor> UpdateAsync(Doctor doctor)
        {
            var dbDoctor = await _context.Doctors.FindAsync(doctor.Id);
            if (dbDoctor == null)
                throw new KeyNotFoundException($"Врач с ID {doctor.Id} не найден");

            dbDoctor.FirstName = doctor.FirstName;
            dbDoctor.LastName = doctor.LastName;
            dbDoctor.Specialty = doctor.Specialization;

            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task DeleteAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
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