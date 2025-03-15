using Gtk;
using System;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Dialogs;

namespace DispensaryApp.UI.Pages
{
    public class AppointmentsPage : Box
    {
        private readonly AppointmentService _appointmentService;
        private readonly TreeView _treeView;
        private readonly ListStore _store;
        private readonly Button _addButton;
        private readonly Button _editButton;
        private readonly Button _cancelButton;
        private readonly ScrolledWindow _scrolledWindow;

        public AppointmentsPage() : base(Orientation.Vertical, 0)
        {
            _appointmentService = new AppointmentService();
            
            // Панель инструментов
            _addButton = new Button("Добавить");
            _editButton = new Button("Редактировать");
            _cancelButton = new Button("Отменить");
            
            PackStart(_addButton, false, false, 0);
            PackStart(_editButton, false, false, 0);
            PackStart(_cancelButton, false, false, 0);

            // Таблица приемов
            _store = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string), 
                                 typeof(string), typeof(string), typeof(string));
            
            _treeView = new TreeView(_store)
            {
                HeadersVisible = true
            };
            
            _treeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            _treeView.AppendColumn("Пациент", new CellRendererText(), "text", 1);
            _treeView.AppendColumn("Врач", new CellRendererText(), "text", 2);
            _treeView.AppendColumn("Дата", new CellRendererText(), "text", 3);
            _treeView.AppendColumn("Время", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Статус", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Тип", new CellRendererText(), "text", 6);
            
            _scrolledWindow = new ScrolledWindow
            {
                Child = _treeView
            };
            
            PackStart(_scrolledWindow, true, true, 0);

            // Подключаем обработчики событий
            _addButton.Clicked += OnAddButtonClicked;
            _editButton.Clicked += OnEditButtonClicked;
            _cancelButton.Clicked += OnCancelButtonClicked;

            // Загружаем данные
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            try
            {
                _store.Clear();
                var appointments = _appointmentService.GetAllAppointments();
                
                foreach (var appointment in appointments)
                {
                    var patientName = appointment.Patient != null 
                        ? $"{appointment.Patient.LastName} {appointment.Patient.FirstName} {appointment.Patient.MiddleName}" 
                        : "Неизвестный пациент";

                    var doctorName = appointment.Doctor != null 
                        ? $"{appointment.Doctor.LastName} {appointment.Doctor.FirstName} {appointment.Doctor.MiddleName}" 
                        : "Неизвестный врач";

                    _store.AppendValues(
                        appointment.Id,
                        patientName,
                        doctorName,
                        appointment.Date.ToString("dd.MM.yyyy"),
                        appointment.Time.ToString(@"hh\:mm"),
                        appointment.Status,
                        appointment.Type
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось загрузить список приемов: {ex.Message}", MessageType.Error);
            }
        }

        private void OnAddButtonClicked(object? sender, EventArgs e)
        {
            if (Toplevel is Window parent)
            {
                var dialog = new AppointmentDialog(parent);
                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    LoadAppointments();
                }
                dialog.Destroy();
            }
        }

        private void OnEditButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                var appointment = _appointmentService.GetAppointmentById(id);
                
                if (appointment != null)
                {
                    if (Toplevel is Window parent)
                    {
                        var dialog = new AppointmentDialog(parent, appointment);
                        if (dialog.Run() == (int)ResponseType.Accept)
                        {
                            LoadAppointments();
                        }
                        dialog.Destroy();
                    }
                }
            }
            else
            {
                ShowMessage("Предупреждение", "Выберите прием для редактирования", MessageType.Warning);
            }
        }

        private void OnCancelButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                try
                {
                    _appointmentService.CancelAppointment(id);
                    LoadAppointments();
                    ShowMessage("Успех", "Прием успешно отменен", MessageType.Info);
                }
                catch (Exception ex)
                {
                    ShowMessage("Ошибка", $"Не удалось отменить прием: {ex.Message}", MessageType.Error);
                }
            }
            else
            {
                ShowMessage("Предупреждение", "Выберите прием для отмены", MessageType.Warning);
            }
        }

        private void ShowMessage(string title, string message, MessageType type)
        {
            var dialog = new MessageDialog(Toplevel as Window, DialogFlags.Modal, type, ButtonsType.Ok, message)
            {
                Title = title
            };
            dialog.Run();
            dialog.Destroy();
        }
    }
} 