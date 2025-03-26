using Gtk;
using System;
using System.Linq;
using System.Threading.Tasks;
using DispensaryApp.Data.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using Microsoft.Extensions.DependencyInjection;

namespace DispensaryApp.UI.Dialogs
{
    public class AppointmentDialog : Dialog
    {
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;
        private readonly DoctorService _doctorService;
        private readonly Appointment _appointment;
        private readonly ComboBox _patientComboBox;
        private readonly ComboBox _doctorComboBox;
        private readonly Calendar _dateCalendar;
        private readonly SpinButton _hoursSpin;
        private readonly SpinButton _minutesSpin;
        private readonly Entry _reasonEntry;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public AppointmentDialog(Window parent, Appointment? appointment = null) : base("Прием", parent, DialogFlags.Modal)
        {
            // Получаем сервисы из DI контейнера
            if (Program.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider не найден");
            }
            _appointmentService = Program.ServiceProvider.GetRequiredService<AppointmentService>();
            _patientService = Program.ServiceProvider.GetRequiredService<PatientService>();
            _doctorService = Program.ServiceProvider.GetRequiredService<DoctorService>();
            
            // Проверка на null
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _appointment = appointment ?? new Appointment
            {
                AppointmentDate = DateTime.Now,
                Reason = "",
                Status = AppointmentStatus.Scheduled
            };

            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 12 };

            // Пациент
            var patientBox = new Box(Orientation.Horizontal, 6);
            patientBox.PackStart(new Label("Пациент:"), false, false, 0);
            _patientComboBox = new ComboBox();
            var patientCell = new CellRendererText();
            _patientComboBox.PackStart(patientCell, true);
            _patientComboBox.AddAttribute(patientCell, "text", 0);
            patientBox.PackStart(_patientComboBox, true, true, 0);
            vbox.PackStart(patientBox, false, false, 0);

            // Врач
            var doctorBox = new Box(Orientation.Horizontal, 6);
            doctorBox.PackStart(new Label("Врач:"), false, false, 0);
            _doctorComboBox = new ComboBox();
            var doctorCell = new CellRendererText();
            _doctorComboBox.PackStart(doctorCell, true);
            _doctorComboBox.AddAttribute(doctorCell, "text", 0);
            doctorBox.PackStart(_doctorComboBox, true, true, 0);
            vbox.PackStart(doctorBox, false, false, 0);

            // Дата и время
            var dateTimeBox = new Box(Orientation.Horizontal, 6);
            dateTimeBox.PackStart(new Label("Дата и время:"), false, false, 0);
            _dateCalendar = new Calendar();
            dateTimeBox.PackStart(_dateCalendar, true, true, 0);

            var timeBox = new Box(Orientation.Vertical, 6);
            _hoursSpin = new SpinButton(0, 23, 1);
            _minutesSpin = new SpinButton(0, 59, 1);
            timeBox.PackStart(_hoursSpin, false, false, 0);
            timeBox.PackStart(_minutesSpin, false, false, 0);
            dateTimeBox.PackStart(timeBox, false, false, 0);

            vbox.PackStart(dateTimeBox, false, false, 0);

            // Причина
            var reasonBox = new Box(Orientation.Horizontal, 6);
            reasonBox.PackStart(new Label("Причина:"), false, false, 0);
            _reasonEntry = new Entry { PlaceholderText = "Причина приема" };
            reasonBox.PackStart(_reasonEntry, true, true, 0);
            vbox.PackStart(reasonBox, false, false, 0);

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

            ContentArea.PackStart(vbox, true, true, 0);

            // Загружаем данные
            LoadDataAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Application.Invoke((sender, e) =>
                    {
                        ShowMessage("Ошибка", "Не удалось загрузить данные: " + t.Exception.Message, MessageType.Error);
                    });
                }
            });

            // Заполняем данные, если это редактирование
            if (appointment != null)
            {
                _reasonEntry.Text = appointment.Reason;
                _dateCalendar.Date = appointment.AppointmentDate;
                _hoursSpin.Value = appointment.AppointmentDate.Hour;
                _minutesSpin.Value = appointment.AppointmentDate.Minute;
            }

            ShowAll();
        }

        private async Task LoadDataAsync()
        {
            var patients = await _patientService.GetAllAsync();
            var doctors = await _doctorService.GetAllAsync();

            Application.Invoke((sender, e) =>
            {
                // Заполняем комбобокс пациентов
                var patientStore = new ListStore(typeof(string), typeof(int));
                foreach (var patient in patients)
                {
                    patientStore.AppendValues($"{patient.LastName} {patient.FirstName} {patient.MiddleName}", patient.Id);
                }
                _patientComboBox.Model = patientStore;

                // Заполняем комбобокс врачей
                var doctorStore = new ListStore(typeof(string), typeof(int));
                foreach (var doctor in doctors)
                {
                    doctorStore.AppendValues($"{doctor.LastName} {doctor.FirstName} {doctor.MiddleName} ({doctor.Specialty})", doctor.Id);
                }
                _doctorComboBox.Model = doctorStore;

                // Если это редактирование, выбираем нужные значения
                if (_appointment.Id != 0)
                {
                    SelectComboBoxItemById(_patientComboBox, _appointment.PatientId);
                    SelectComboBoxItemById(_doctorComboBox, _appointment.DoctorId);
                }
            });
        }

        private void SelectComboBoxItemById(ComboBox comboBox, int id)
        {
            var model = (ListStore)comboBox.Model;
            TreeIter iter;
            if (model.GetIterFirst(out iter))
            {
                do
                {
                    if ((int)model.GetValue(iter, 1) == id)
                    {
                        comboBox.SetActiveIter(iter);
                        break;
                    }
                } while (model.IterNext(ref iter));
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                TreeIter iter;
                if (!_patientComboBox.GetActiveIter(out iter))
                {
                    ShowMessage("Ошибка", "Выберите пациента", MessageType.Error);
                    return;
                }
                _appointment.PatientId = (int)((ListStore)_patientComboBox.Model).GetValue(iter, 1);

                if (!_doctorComboBox.GetActiveIter(out iter))
                {
                    ShowMessage("Ошибка", "Выберите врача", MessageType.Error);
                    return;
                }
                _appointment.DoctorId = (int)((ListStore)_doctorComboBox.Model).GetValue(iter, 1);

                var date = _dateCalendar.Date;
                _appointment.AppointmentDate = new DateTime(
                    date.Year,
                    date.Month,
                    date.Day,
                    (int)_hoursSpin.Value,
                    (int)_minutesSpin.Value,
                    0
                );

                _appointment.Reason = _reasonEntry.Text;

                if (_appointment.Id == 0)
                {
                    await _appointmentService.AddAsync(_appointment);
                }
                else
                {
                    await _appointmentService.UpdateAsync(_appointment);
                }

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