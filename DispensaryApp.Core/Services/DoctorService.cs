using System;
using System.Collections.Generic;
using System.Linq;
using DispensaryApp.Core.Models;

namespace DispensaryApp.Core.Services
{
    public class DoctorService
    {
        private readonly List<Doctor> _doctors;

        public DoctorService()
        {
            _doctors = new List<Doctor>();
        }

        public IEnumerable<Doctor> GetAllDoctors()
        {
            return _doctors;
        }

        public Doctor GetDoctorById(int id)
        {
            return _doctors.FirstOrDefault(d => d.Id == id);
        }

        public void SaveDoctor(Doctor doctor)
        {
            if (doctor.Id == 0)
            {
                doctor.Id = _doctors.Count + 1;
                _doctors.Add(doctor);
            }
            else
            {
                var existingDoctor = _doctors.FirstOrDefault(d => d.Id == doctor.Id);
                if (existingDoctor != null)
                {
                    var index = _doctors.IndexOf(existingDoctor);
                    _doctors[index] = doctor;
                }
            }
        }

        public void DeleteDoctor(int id)
        {
            var doctor = _doctors.FirstOrDefault(d => d.Id == id);
            if (doctor != null)
            {
                _doctors.Remove(doctor);
            }
        }
    }
} 