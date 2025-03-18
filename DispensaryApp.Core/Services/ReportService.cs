using System;
using System.Linq;
using System.Text;
using DispensaryApp.Core.Models;
using DispensaryApp.Data;

namespace DispensaryApp.Core.Services
{
    public class ReportService
    {
        private readonly DoctorService _doctorService;
        private readonly PatientService _patientService;
        private readonly AppointmentService _appointmentService;

        public ReportService(DispensaryDbContext context)
        {
            _doctorService = new DoctorService(context);
            _patientService = new PatientService(context);
            _appointmentService = new AppointmentService(context);
        }

        public string GenerateAppointmentsReport()
        {
            var appointments = _appointmentService.GetAllAppointments();
            var sb = new StringBuilder();

            sb.AppendLine("Отчет по приемам");
            sb.AppendLine("=================");
            sb.AppendLine();

            foreach (var appointment in appointments)
            {
                sb.AppendLine($"ID: {appointment.Id}");
                sb.AppendLine($"Пациент: {appointment.Patient?.LastName} {appointment.Patient?.FirstName}");
                sb.AppendLine($"Врач: {appointment.Doctor?.LastName} {appointment.Doctor?.FirstName}");
                sb.AppendLine($"Дата: {appointment.Date.ToShortDateString()}");
                sb.AppendLine($"Время: {appointment.Time}");
                sb.AppendLine($"Статус: {appointment.Status}");
                sb.AppendLine($"Тип: {appointment.Type}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string GeneratePatientsReport()
        {
            var patients = _patientService.GetAllPatients();
            var sb = new StringBuilder();

            sb.AppendLine("Отчет по пациентам");
            sb.AppendLine("==================");
            sb.AppendLine();

            foreach (var patient in patients)
            {
                sb.AppendLine($"ID: {patient.Id}");
                sb.AppendLine($"ФИО: {patient.LastName} {patient.FirstName} {patient.MiddleName}");
                sb.AppendLine($"Полис ОМС: {patient.InsurancePolicy}");
                sb.AppendLine($"Телефон: {patient.Phone}");
                sb.AppendLine($"Email: {patient.Email}");
                sb.AppendLine($"Адрес: {patient.Address}");
                sb.AppendLine($"Дата рождения: {patient.BirthDate.ToShortDateString()}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string GenerateDoctorsReport()
        {
            var doctors = _doctorService.GetAllDoctors();
            var sb = new StringBuilder();

            sb.AppendLine("Отчет по врачам");
            sb.AppendLine("===============");
            sb.AppendLine();

            foreach (var doctor in doctors)
            {
                sb.AppendLine($"ID: {doctor.Id}");
                sb.AppendLine($"ФИО: {doctor.LastName} {doctor.FirstName} {doctor.MiddleName}");
                sb.AppendLine($"Специализация: {doctor.Specialization}");
                sb.AppendLine($"Номер лицензии: {doctor.LicenseNumber}");
                sb.AppendLine($"Телефон: {doctor.Phone}");
                sb.AppendLine($"Email: {doctor.Email}");
                sb.AppendLine($"График работы: {doctor.Schedule}");
                sb.AppendLine($"Дата приема на работу: {doctor.HireDate.ToShortDateString()}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string GenerateIncomeReport()
        {
            var appointments = _appointmentService.GetAllAppointments();
            var sb = new StringBuilder();

            sb.AppendLine("Отчет по доходам");
            sb.AppendLine("================");
            sb.AppendLine();

            var totalIncome = appointments.Where(a => a.Status == "Завершен").Sum(a => a.Cost);
            sb.AppendLine($"Общий доход: {totalIncome:C}");
            sb.AppendLine();

            sb.AppendLine("Доход по типам приема:");
            var incomeByType = appointments
                .Where(a => a.Status == "Завершен")
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Total = g.Sum(a => a.Cost) });

            foreach (var item in incomeByType)
            {
                sb.AppendLine($"{item.Type}: {item.Total:C}");
            }

            return sb.ToString();
        }
    }
} 