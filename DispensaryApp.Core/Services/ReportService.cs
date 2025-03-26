using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.Data;
using DispensaryApp.Data.Models;
using System.IO;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace DispensaryApp.Core.Services
{
    public class ReportService
    {
        private readonly DoctorService _doctorService;
        private readonly PatientService _patientService;
        private readonly AppointmentService _appointmentService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            ILogger<ReportService> logger,
            ILogger<DoctorService> doctorLogger,
            ILogger<PatientService> patientLogger,
            ILogger<AppointmentService> appointmentLogger)
        {
            _logger = logger;
            _doctorService = new DoctorService(doctorLogger);
            _patientService = new PatientService(patientLogger);
            _appointmentService = new AppointmentService(appointmentLogger);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsAsync()
        {
            return await _doctorService.GetAllAsync();
        }

        public async Task<IEnumerable<Patient>> GetPatientsAsync()
        {
            return await _patientService.GetAllAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync()
        {
            return await _appointmentService.GetAllAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            var appointments = await GetAppointmentsAsync();
            return appointments.Where(a => a.DoctorId == doctorId);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            var appointments = await GetAppointmentsAsync();
            return appointments.Where(a => a.PatientId == patientId);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var appointments = await GetAppointmentsAsync();
            return appointments.Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(AppointmentStatus status)
        {
            var appointments = await GetAppointmentsAsync();
            return appointments.Where(a => a.Status == status);
        }

        public async Task<IEnumerable<ReportItem>> GenerateReportAsync(int reportType)
        {
            var appointments = await _appointmentService.GetAllAsync();
            var result = new List<ReportItem>();

            switch (reportType)
            {
                case 0: // Ежедневный отчет
                    var dailyGroups = appointments
                        .Where(a => a.Date.Date == DateTime.Today)
                        .GroupBy(a => new { a.Doctor, a.Date.Date });

                    foreach (var group in dailyGroups)
                    {
                        result.Add(new ReportItem
                        {
                            Date = group.Key.Date,
                            DoctorName = $"{group.Key.Doctor?.LastName} {group.Key.Doctor?.FirstName}",
                            AppointmentsCount = group.Count(),
                            Status = "Ежедневный"
                        });
                    }
                    break;

                case 1: // Еженедельный отчет
                    var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    var weeklyGroups = appointments
                        .Where(a => a.Date.Date >= startOfWeek && a.Date.Date <= startOfWeek.AddDays(6))
                        .GroupBy(a => new { a.Doctor, WeekStart = startOfWeek });

                    foreach (var group in weeklyGroups)
                    {
                        result.Add(new ReportItem
                        {
                            Date = group.Key.WeekStart,
                            DoctorName = $"{group.Key.Doctor?.LastName} {group.Key.Doctor?.FirstName}",
                            AppointmentsCount = group.Count(),
                            Status = "Еженедельный"
                        });
                    }
                    break;

                case 2: // Ежемесячный отчет
                    var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    var monthlyGroups = appointments
                        .Where(a => a.Date.Date >= startOfMonth && a.Date.Date <= startOfMonth.AddMonths(1).AddDays(-1))
                        .GroupBy(a => new { a.Doctor, MonthStart = startOfMonth });

                    foreach (var group in monthlyGroups)
                    {
                        result.Add(new ReportItem
                        {
                            Date = group.Key.MonthStart,
                            DoctorName = $"{group.Key.Doctor?.LastName} {group.Key.Doctor?.FirstName}",
                            AppointmentsCount = group.Count(),
                            Status = "Ежемесячный"
                        });
                    }
                    break;
            }

            return result;
        }

        public async Task ExportToExcelAsync()
        {
            try
            {
                _logger.LogInformation("Начало экспорта отчета в Excel");
                var appointments = await GetAppointmentsAsync();
                var reportItems = appointments.Select(a => new ReportItem
                {
                    Date = a.AppointmentDate,
                    DoctorName = $"{a.Doctor?.LastName} {a.Doctor?.FirstName}",
                    AppointmentsCount = 1,
                    Status = a.Status.ToString()
                });

                var filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Отчет");

                    // Заголовки
                    worksheet.Cells[1, 1].Value = "Дата";
                    worksheet.Cells[1, 2].Value = "Врач";
                    worksheet.Cells[1, 3].Value = "Количество приемов";
                    worksheet.Cells[1, 4].Value = "Статус";

                    // Стиль заголовков
                    var headerRange = worksheet.Cells[1, 1, 1, 4];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    // Данные
                    var row = 2;
                    foreach (var item in reportItems)
                    {
                        worksheet.Cells[row, 1].Value = item.Date.ToString("dd.MM.yyyy HH:mm");
                        worksheet.Cells[row, 2].Value = item.DoctorName;
                        worksheet.Cells[row, 3].Value = item.AppointmentsCount;
                        worksheet.Cells[row, 4].Value = item.Status;
                        row++;
                    }

                    // Автоподбор ширины колонок
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Сохранение файла
                    await package.SaveAsAsync(new FileInfo(filePath));
                }

                _logger.LogInformation($"Отчет успешно экспортирован в файл: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при экспорте отчета в Excel");
                throw;
            }
        }

        public void ExportToCsv(string filename, IEnumerable<ReportItem> items)
        {
            var lines = new List<string> { "Дата,Врач,Количество приемов,Статус" };

            foreach (var item in items)
            {
                lines.Add($"{item.Date.ToShortDateString()},{item.DoctorName},{item.AppointmentsCount},{item.Status}");
            }

            File.WriteAllLines(filename, lines);
        }
    }

    public class ReportItem
    {
        public DateTime Date { get; set; }
        public string DoctorName { get; set; } = "";
        public int AppointmentsCount { get; set; }
        public string Status { get; set; } = "";
    }
} 