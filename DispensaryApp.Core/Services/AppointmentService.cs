using System;
using System.Collections.Generic;
using System.Linq;
using DispensaryApp.Core.Models;

namespace DispensaryApp.Core.Services
{
    public class AppointmentService
    {
        private readonly List<Appointment> _appointments;
        private readonly DoctorService _doctorService;
        private readonly PatientService _patientService;

        public AppointmentService()
        {
            _appointments = new List<Appointment>();
            _doctorService = new DoctorService();
            _patientService = new PatientService();
        }

        public IEnumerable<Appointment> GetAllAppointments()
        {
            return _appointments;
        }

        public Appointment GetAppointmentById(int id)
        {
            return _appointments.FirstOrDefault(a => a.Id == id);
        }

        public void SaveAppointment(Appointment appointment)
        {
            if (appointment.Id == 0)
            {
                appointment.Id = _appointments.Count + 1;
                _appointments.Add(appointment);
            }
            else
            {
                var existingAppointment = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
                if (existingAppointment != null)
                {
                    var index = _appointments.IndexOf(existingAppointment);
                    _appointments[index] = appointment;
                }
            }
        }

        public void CancelAppointment(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment != null)
            {
                appointment.Status = "Отменен";
            }
        }
    }
} 