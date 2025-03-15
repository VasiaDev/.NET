using System;
using System.Collections.Generic;
using System.Linq;
using DispensaryApp.Core.Models;

namespace DispensaryApp.Core.Services
{
    public class PatientService
    {
        private readonly List<Patient> _patients;

        public PatientService()
        {
            _patients = new List<Patient>();
        }

        public IEnumerable<Patient> GetAllPatients()
        {
            return _patients;
        }

        public Patient GetPatientById(int id)
        {
            return _patients.FirstOrDefault(p => p.Id == id);
        }

        public void SavePatient(Patient patient)
        {
            if (patient.Id == 0)
            {
                patient.Id = _patients.Count + 1;
                _patients.Add(patient);
            }
            else
            {
                var existingPatient = _patients.FirstOrDefault(p => p.Id == patient.Id);
                if (existingPatient != null)
                {
                    var index = _patients.IndexOf(existingPatient);
                    _patients[index] = patient;
                }
            }
        }

        public void DeletePatient(int id)
        {
            var patient = _patients.FirstOrDefault(p => p.Id == id);
            if (patient != null)
            {
                _patients.Remove(patient);
            }
        }
    }
} 