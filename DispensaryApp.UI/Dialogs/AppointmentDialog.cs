using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using DispensaryApp.Data;

namespace DispensaryApp.UI.Dialogs
{
    public class AppointmentDialog : Dialog
    {
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;
        private readonly DoctorService _doctorService;
        private readonly Appointment _appointment;
        private readonly ComboBoxText _patientComboBox;
        private readonly ComboBoxText _doctorComboBox;
        private readonly Calendar _dateCalendar;
        private readonly SpinButton _hourSpinButton;
        private readonly SpinButton _minuteSpinButton;
        private readonly ComboBoxText _statusComboBox;
        private readonly ComboBoxText _typeComboBox;
        private readonly TextView _notesTextView;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public AppointmentDialog(Window parent, DispensaryDbContext context, Appointment? appointment = null) : base("Запись", parent, DialogFlags.Modal)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _appointmentService = new AppointmentService(context);
            _patientService = new PatientService(context);
            _doctorService = new DoctorService(context);
            _appointment = appointment ?? new Appointment();
            
            var vbox = new Box(Orientation.Vertical, 6);
            
            // Пациент и врач
            var patientBox = new Box(Orientation.Horizontal, 6);
            patientBox.PackStart(new Label("Пациент:"), false, false, 0);
            _patientComboBox = new ComboBoxText();
            patientBox.PackStart(_patientComboBox, true, true, 0);
            vbox.PackStart(patientBox, false, false, 0);

            var doctorBox = new Box(Orientation.Horizontal, 6);
            doctorBox.PackStart(new Label("Врач:"), false, false, 0);
            _doctorComboBox = new ComboBoxText();
            doctorBox.PackStart(_doctorComboBox, true, true, 0);
            vbox.PackStart(doctorBox, false, false, 0);

            // Дата и время
            var dateBox = new Box(Orientation.Horizontal, 6);
            dateBox.PackStart(new Label("Дата:"), false, false, 0);
            _dateCalendar = new Calendar();
            dateBox.PackStart(_dateCalendar, true, true, 0);
            vbox.PackStart(dateBox, false, false, 0);

            var timeBox = new Box(Orientation.Horizontal, 6);
            timeBox.PackStart(new Label("Время:"), false, false, 0);
            _hourSpinButton = new SpinButton(0, 23, 1);
            _minuteSpinButton = new SpinButton(0, 59, 1);
            timeBox.PackStart(_hourSpinButton, false, false, 0);
            timeBox.PackStart(new Label(":"), false, false, 0);
            timeBox.PackStart(_minuteSpinButton, false, false, 0);
            vbox.PackStart(timeBox, false, false, 0);

            // Статус и тип приема
            var statusBox = new Box(Orientation.Horizontal, 6);
            statusBox.PackStart(new Label("Статус:"), false, false, 0);
            _statusComboBox = new ComboBoxText();
            _statusComboBox.AppendText("Запланирован");
            _statusComboBox.AppendText("Завершен");
            _statusComboBox.AppendText("Отменен");
            statusBox.PackStart(_statusComboBox, true, true, 0);
            vbox.PackStart(statusBox, false, false, 0);

            var typeBox = new Box(Orientation.Horizontal, 6);
            typeBox.PackStart(new Label("Тип приема:"), false, false, 0);
            _typeComboBox = new ComboBoxText();
            _typeComboBox.AppendText("Первичный");
            _typeComboBox.AppendText("Повторный");
            _typeComboBox.AppendText("Консультация");
            typeBox.PackStart(_typeComboBox, true, true, 0);
            vbox.PackStart(typeBox, false, false, 0);

            // Примечания
            var notesBox = new Box(Orientation.Vertical, 6);
            notesBox.PackStart(new Label("Примечания:"), false, false, 0);
            _notesTextView = new TextView();
            var notesScrolledWindow = new ScrolledWindow
            {
                HeightRequest = 100,
                Child = _notesTextView
            };
            notesBox.PackStart(notesScrolledWindow, true, true, 0);
            vbox.PackStart(notesBox, false, false, 0);

            // Кнопки
            var buttonBox = new Box(Orientation.Horizontal, 6);
            _saveButton = new Button("Сохранить");
            _cancelButton = new Button("Отмена");
            buttonBox.PackEnd(_saveButton, false, false, 0);
            buttonBox.PackEnd(_cancelButton, false, false, 0);
            vbox.PackStart(buttonBox, false, false, 0);

            // Применяем стили
            StyleManager.ApplyButtonStyle(_saveButton);
            StyleManager.ApplyButtonStyle(_cancelButton);

            // Подключаем обработчики событий
            _saveButton.Clicked += OnSaveClicked;
            _cancelButton.Clicked += OnCancelClicked;

            // Загружаем данные в комбобоксы
            LoadComboBoxes();

            // Заполняем данные, если это редактирование
            if (appointment != null)
            {
                _patientComboBox.Active = GetPatientIndex(appointment.PatientId);
                _doctorComboBox.Active = GetDoctorIndex(appointment.DoctorId);
                _dateCalendar.Date = appointment.Date;
                _hourSpinButton.Value = appointment.Time.Hours;
                _minuteSpinButton.Value = appointment.Time.Minutes;
                _statusComboBox.Active = GetStatusIndex(appointment.Status);
                _typeComboBox.Active = GetTypeIndex(appointment.Type);
                _notesTextView.Buffer.Text = appointment.Notes;
            }

            ContentArea.PackStart(vbox, true, true, 0);
            ShowAll();
        }

        private void LoadComboBoxes()
        {
            try
            {
                var patients = _patientService.GetAllPatients();
                foreach (var patient in patients)
                {
                    _patientComboBox.AppendText($"{patient.LastName} {patient.FirstName} {patient.MiddleName}");
                }

                var doctors = _doctorService.GetAllDoctors();
                foreach (var doctor in doctors)
                {
                    _doctorComboBox.AppendText($"{doctor.LastName} {doctor.FirstName} {doctor.MiddleName} - {doctor.Specialization}");
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось загрузить данные: {ex.Message}", MessageType.Error);
            }
        }

        private int GetPatientIndex(int patientId)
        {
            var patients = _patientService.GetAllPatients().ToList();
            for (int i = 0; i < patients.Count; i++)
            {
                if (patients[i].Id == patientId)
                    return i;
            }
            return 0;
        }

        private int GetDoctorIndex(int doctorId)
        {
            var doctors = _doctorService.GetAllDoctors().ToList();
            for (int i = 0; i < doctors.Count; i++)
            {
                if (doctors[i].Id == doctorId)
                    return i;
            }
            return 0;
        }

        private int GetStatusIndex(string? status)
        {
            return status switch
            {
                "Запланирован" => 0,
                "Завершен" => 1,
                "Отменен" => 2,
                _ => 0
            };
        }

        private int GetTypeIndex(string? type)
        {
            return type switch
            {
                "Первичный" => 0,
                "Повторный" => 1,
                "Консультация" => 2,
                _ => 0
            };
        }

        private void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                var patients = _patientService.GetAllPatients().ToList();
                var doctors = _doctorService.GetAllDoctors().ToList();

                _appointment.PatientId = patients[_patientComboBox.Active].Id;
                _appointment.DoctorId = doctors[_doctorComboBox.Active].Id;
                _appointment.Date = _dateCalendar.Date;
                _appointment.Time = new TimeSpan((int)_hourSpinButton.Value, (int)_minuteSpinButton.Value, 0);
                _appointment.Status = _statusComboBox.ActiveText;
                _appointment.Type = _typeComboBox.ActiveText;
                _appointment.Notes = _notesTextView.Buffer.Text;

                _appointmentService.SaveAppointment(_appointment);
                Respond(ResponseType.Accept);
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось сохранить данные приема: {ex.Message}", MessageType.Error);
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            Respond(ResponseType.Cancel);
        }

        private void ShowMessage(string title, string message, MessageType type)
        {
            var dialog = new MessageDialog(this, DialogFlags.Modal, type, ButtonsType.Ok, message)
            {
                Title = title
            };
            dialog.Run();
            dialog.Destroy();
        }
    }
} 