using Gtk;
using System;
using DispensaryApp.Data.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using Microsoft.Extensions.Logging;

namespace DispensaryApp.UI.Dialogs
{
    public class DoctorDialog : Dialog
    {
        private readonly DoctorService _doctorService;
        private readonly Doctor _doctor;
        private readonly Entry _lastNameEntry;
        private readonly Entry _firstNameEntry;
        private readonly Entry _middleNameEntry;
        private readonly Entry _specialtyEntry;
        private readonly Entry _specializationEntry;
        private readonly Entry _licenseNumberEntry;
        private readonly Entry _phoneEntry;
        private readonly Entry _emailEntry;
        private readonly Entry _scheduleEntry;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public DoctorDialog(Window parent, Doctor? doctor = null) : base("Врач", parent, DialogFlags.Modal)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });
            var logger = loggerFactory.CreateLogger<DoctorService>();
            _doctorService = new DoctorService(logger);
            
            // Проверка на null
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _doctor = doctor ?? new Doctor
            {
                LastName = "",
                FirstName = "",
                MiddleName = "",
                Specialty = "",
                Specialization = "",
                LicenseNumber = "",
                Phone = "",
                Email = "",
                Schedule = ""
            };

            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 12 };
            
            // ФИО
            var nameBox = new Box(Orientation.Horizontal, 6);
            _lastNameEntry = new Entry { PlaceholderText = "Фамилия" };
            _firstNameEntry = new Entry { PlaceholderText = "Имя" };
            _middleNameEntry = new Entry { PlaceholderText = "Отчество" };
            nameBox.PackStart(_lastNameEntry, true, true, 0);
            nameBox.PackStart(_firstNameEntry, true, true, 0);
            nameBox.PackStart(_middleNameEntry, true, true, 0);
            vbox.PackStart(nameBox, false, false, 0);

            // Специальность и специализация
            var specialtyBox = new Box(Orientation.Horizontal, 6);
            _specialtyEntry = new Entry { PlaceholderText = "Специальность" };
            _specializationEntry = new Entry { PlaceholderText = "Специализация" };
            specialtyBox.PackStart(_specialtyEntry, true, true, 0);
            specialtyBox.PackStart(_specializationEntry, true, true, 0);
            vbox.PackStart(specialtyBox, false, false, 0);

            // Лицензия
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
                _middleNameEntry.Text = doctor.MiddleName ?? "";
                _specialtyEntry.Text = doctor.Specialty;
                _specializationEntry.Text = doctor.Specialization ?? "";
                _licenseNumberEntry.Text = doctor.LicenseNumber ?? "";
                _phoneEntry.Text = doctor.Phone ?? "";
                _emailEntry.Text = doctor.Email ?? "";
                _scheduleEntry.Text = doctor.Schedule ?? "";
            }

            ContentArea.PackStart(vbox, true, true, 0);
            ShowAll();
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                _doctor.LastName = _lastNameEntry.Text;
                _doctor.FirstName = _firstNameEntry.Text;
                _doctor.MiddleName = _middleNameEntry.Text;
                _doctor.Specialty = _specialtyEntry.Text;
                _doctor.Specialization = _specializationEntry.Text;
                _doctor.LicenseNumber = _licenseNumberEntry.Text;
                _doctor.Phone = _phoneEntry.Text;
                _doctor.Email = _emailEntry.Text;
                _doctor.Schedule = _scheduleEntry.Text;

                if (_doctor.Id == 0)
                {
                    await _doctorService.AddAsync(_doctor);
                }
                else
                {
                    await _doctorService.UpdateAsync(_doctor);
                }
                
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