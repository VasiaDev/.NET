using Gtk;
using System;
using DispensaryApp.Data.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace DispensaryApp.UI.Dialogs
{
    public class PatientDialog : Dialog
    {
        private readonly PatientService _patientService;
        private readonly Patient _patient;
        private readonly Entry _lastNameEntry;
        private readonly Entry _firstNameEntry;
        private readonly Entry _middleNameEntry;
        private readonly Entry _insurancePolicyEntry;
        private readonly Entry _phoneEntry;
        private readonly Entry _emailEntry;
        private readonly Entry _genderEntry;
        private readonly Calendar _birthDateCalendar;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public PatientDialog(Window parent, Patient? patient = null) : base("Пациент", parent, DialogFlags.Modal)
        {
            // Получаем сервис из DI контейнера
            if (Program.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider не найден");
            }
            _patientService = Program.ServiceProvider.GetRequiredService<PatientService>();
            
            // Проверка на null
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _patient = patient ?? new Patient
            {
                LastName = "",
                FirstName = "",
                MiddleName = "",
                Gender = "",
                DateOfBirth = DateTime.Now
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

            // Страховой полис
            var insuranceBox = new Box(Orientation.Horizontal, 6);
            insuranceBox.PackStart(new Label("Страховой полис:"), false, false, 0);
            _insurancePolicyEntry = new Entry { PlaceholderText = "Номер страхового полиса" };
            insuranceBox.PackStart(_insurancePolicyEntry, true, true, 0);
            vbox.PackStart(insuranceBox, false, false, 0);

            // Контакты
            var contactsBox = new Box(Orientation.Horizontal, 6);
            _phoneEntry = new Entry { PlaceholderText = "Телефон" };
            _emailEntry = new Entry { PlaceholderText = "Email" };
            contactsBox.PackStart(_phoneEntry, true, true, 0);
            contactsBox.PackStart(_emailEntry, true, true, 0);
            vbox.PackStart(contactsBox, false, false, 0);

            // Пол
            var genderBox = new Box(Orientation.Horizontal, 6);
            genderBox.PackStart(new Label("Пол:"), false, false, 0);
            _genderEntry = new Entry { PlaceholderText = "М/Ж" };
            genderBox.PackStart(_genderEntry, true, true, 0);
            vbox.PackStart(genderBox, false, false, 0);

            // Дата рождения
            var birthDateBox = new Box(Orientation.Horizontal, 6);
            birthDateBox.PackStart(new Label("Дата рождения:"), false, false, 0);
            _birthDateCalendar = new Calendar();
            birthDateBox.PackStart(_birthDateCalendar, true, true, 0);
            vbox.PackStart(birthDateBox, false, false, 0);

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
            if (patient != null)
            {
                _lastNameEntry.Text = patient.LastName;
                _firstNameEntry.Text = patient.FirstName;
                _middleNameEntry.Text = patient.MiddleName ?? "";
                _insurancePolicyEntry.Text = patient.InsurancePolicy ?? "";
                _phoneEntry.Text = patient.Phone ?? "";
                _emailEntry.Text = patient.Email ?? "";
                _genderEntry.Text = patient.Gender;
                _birthDateCalendar.Date = patient.DateOfBirth;
            }

            ContentArea.PackStart(vbox, true, true, 0);
            ShowAll();
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                _patient.LastName = _lastNameEntry.Text;
                _patient.FirstName = _firstNameEntry.Text;
                _patient.MiddleName = _middleNameEntry.Text;
                _patient.InsurancePolicy = _insurancePolicyEntry.Text;
                _patient.Phone = _phoneEntry.Text;
                _patient.Email = _emailEntry.Text;
                _patient.Gender = _genderEntry.Text;
                _patient.DateOfBirth = _birthDateCalendar.Date;

                if (_patient.Id == 0)
                {
                    await _patientService.AddAsync(_patient);
                }
                else
                {
                    await _patientService.UpdateAsync(_patient);
                }
                
                Respond(ResponseType.Accept);
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось сохранить данные пациента: {ex.Message}", MessageType.Error);
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