using Gtk;
using System;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using DispensaryApp.Data;

namespace DispensaryApp.UI.Dialogs
{
    public class DoctorDialog : Dialog
    {
        private readonly DoctorService _doctorService;
        private readonly Doctor _doctor;
        private readonly Entry _lastNameEntry;
        private readonly Entry _firstNameEntry;
        private readonly Entry _middleNameEntry;
        private readonly Entry _specializationEntry;
        private readonly Entry _licenseNumberEntry;
        private readonly Entry _phoneEntry;
        private readonly Entry _emailEntry;
        private readonly Entry _scheduleEntry;
        private readonly Calendar _hireDateCalendar;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public DoctorDialog(Window parent, DispensaryDbContext context, Doctor? doctor = null) : base("Доктор", parent, DialogFlags.Modal)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _doctorService = new DoctorService(context);
            _doctor = doctor ?? new Doctor
            {
                LastName = "",
                FirstName = "",
                MiddleName = "",
                Specialization = "",
                LicenseNumber = "",
                Phone = "",
                Email = "",
                Schedule = "",
                HireDate = DateTime.Now
            };
            
            var vbox = new Box(Orientation.Vertical, 6);
            
            // ФИО
            var nameBox = new Box(Orientation.Horizontal, 6);
            _lastNameEntry = new Entry { PlaceholderText = "Фамилия" };
            _firstNameEntry = new Entry { PlaceholderText = "Имя" };
            _middleNameEntry = new Entry { PlaceholderText = "Отчество" };
            nameBox.PackStart(_lastNameEntry, true, true, 0);
            nameBox.PackStart(_firstNameEntry, true, true, 0);
            nameBox.PackStart(_middleNameEntry, true, true, 0);
            vbox.PackStart(nameBox, false, false, 0);

            // Специализация и лицензия
            var specializationBox = new Box(Orientation.Horizontal, 6);
            specializationBox.PackStart(new Label("Специализация:"), false, false, 0);
            _specializationEntry = new Entry { PlaceholderText = "Специализация" };
            specializationBox.PackStart(_specializationEntry, true, true, 0);
            vbox.PackStart(specializationBox, false, false, 0);

            var licenseBox = new Box(Orientation.Horizontal, 6);
            licenseBox.PackStart(new Label("Номер лицензии:"), false, false, 0);
            _licenseNumberEntry = new Entry { PlaceholderText = "Номер лицензии" };
            licenseBox.PackStart(_licenseNumberEntry, true, true, 0);
            vbox.PackStart(licenseBox, false, false, 0);

            // Контакты
            var contactsBox = new Box(Orientation.Horizontal, 6);
            _phoneEntry = new Entry { PlaceholderText = "Телефон" };
            _emailEntry = new Entry { PlaceholderText = "Email" };
            contactsBox.PackStart(_phoneEntry, true, true, 0);
            contactsBox.PackStart(_emailEntry, true, true, 0);
            vbox.PackStart(contactsBox, false, false, 0);

            // График работы
            var scheduleBox = new Box(Orientation.Horizontal, 6);
            scheduleBox.PackStart(new Label("График работы:"), false, false, 0);
            _scheduleEntry = new Entry { PlaceholderText = "График работы" };
            scheduleBox.PackStart(_scheduleEntry, true, true, 0);
            vbox.PackStart(scheduleBox, false, false, 0);

            // Дата приема на работу
            var hireDateBox = new Box(Orientation.Horizontal, 6);
            hireDateBox.PackStart(new Label("Дата приема на работу:"), false, false, 0);
            _hireDateCalendar = new Calendar();
            hireDateBox.PackStart(_hireDateCalendar, true, true, 0);
            vbox.PackStart(hireDateBox, false, false, 0);

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

            // Заполняем данные, если это редактирование
            if (doctor != null)
            {
                _lastNameEntry.Text = doctor.LastName;
                _firstNameEntry.Text = doctor.FirstName;
                _middleNameEntry.Text = doctor.MiddleName;
                _specializationEntry.Text = doctor.Specialization;
                _licenseNumberEntry.Text = doctor.LicenseNumber;
                _phoneEntry.Text = doctor.Phone;
                _emailEntry.Text = doctor.Email;
                _scheduleEntry.Text = doctor.Schedule;
                _hireDateCalendar.Date = doctor.HireDate;
            }

            ContentArea.PackStart(vbox, true, true, 0);
            ShowAll();
        }

        private void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                _doctor.LastName = _lastNameEntry.Text;
                _doctor.FirstName = _firstNameEntry.Text;
                _doctor.MiddleName = _middleNameEntry.Text;
                _doctor.Specialization = _specializationEntry.Text;
                _doctor.LicenseNumber = _licenseNumberEntry.Text;
                _doctor.Phone = _phoneEntry.Text;
                _doctor.Email = _emailEntry.Text;
                _doctor.Schedule = _scheduleEntry.Text;
                _doctor.HireDate = _hireDateCalendar.Date;

                _doctorService.SaveDoctor(_doctor);
                Respond(ResponseType.Accept);
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось сохранить данные врача: {ex.Message}", MessageType.Error);
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