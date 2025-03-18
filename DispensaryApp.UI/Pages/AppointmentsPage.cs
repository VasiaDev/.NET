using Gtk;
using System;
using System.Threading.Tasks;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Dialogs;
using DispensaryApp.UI.Styles;
using DispensaryApp.Data;

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

        public AppointmentsPage(DispensaryDbContext context) : base(Orientation.Vertical, 0)
        {
            _appointmentService = new AppointmentService(context);
            
            // Панель инструментов
            var toolbar = new Box(Orientation.Horizontal, 6) { MarginStart = 6, MarginEnd = 6, MarginTop = 6, MarginBottom = 6 };
            
            _addButton = new Button("Добавить");
            _editButton = new Button("Редактировать");
            _cancelButton = new Button("Отменить");
            
            StyleManager.ApplyButtonStyle(_addButton);
            StyleManager.ApplyButtonStyle(_editButton);
            StyleManager.ApplyButtonStyle(_cancelButton);
            
            toolbar.PackStart(_addButton, false, false, 0);
            toolbar.PackStart(_editButton, false, false, 0);
            toolbar.PackStart(_cancelButton, false, false, 0);
            toolbar.PackEnd(new Label(""), true, true, 0);
            
            PackStart(toolbar, false, false, 0);

            // Таблица приемов
            _store = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string), 
                                 typeof(string), typeof(string), typeof(string));
            
            _treeView = new TreeView(_store)
            {
                HeadersVisible = true,
                Reorderable = true
            };
            
            foreach (var column in _treeView.Columns)
            {
                column.Resizable = true;
                column.Clickable = true;
            }
            
            StyleManager.ApplyTreeViewStyle(_treeView);
            
            _treeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            _treeView.AppendColumn("Пациент", new CellRendererText(), "text", 1);
            _treeView.AppendColumn("Врач", new CellRendererText(), "text", 2);
            _treeView.AppendColumn("Дата", new CellRendererText(), "text", 3);
            _treeView.AppendColumn("Время", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Статус", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Тип", new CellRendererText(), "text", 6);
            
            _scrolledWindow = new ScrolledWindow
            {
                Child = _treeView,
                ShadowType = ShadowType.In
            };
            
            PackStart(_scrolledWindow, true, true, 0);

            // Подключаем обработчики событий
            _addButton.Clicked += OnAddButtonClicked;
            _editButton.Clicked += OnEditButtonClicked;
            _cancelButton.Clicked += OnCancelButtonClicked;

            // Загружаем данные
            LoadAppointments();
        }

        private async void LoadAppointments()
        {
            try
            {
                _store.Clear();
                var appointments = await _appointmentService.GetAllAsync();
                
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

        private async void OnAddButtonClicked(object? sender, EventArgs e)
        {
            if (Toplevel is Window parent)
            {
                var dialog = new AppointmentDialog(parent, _appointmentService.Context);
                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    await Task.Run(() => LoadAppointments());
                }
                dialog.Destroy();
            }
        }

        private async void OnEditButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                var appointment = await _appointmentService.GetByIdAsync(id);
                
                if (appointment != null)
                {
                    if (Toplevel is Window parent)
                    {
                        var dialog = new AppointmentDialog(parent, _appointmentService.Context, appointment);
                        if (dialog.Run() == (int)ResponseType.Accept)
                        {
                            await Task.Run(() => LoadAppointments());
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

        private async void OnCancelButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                try
                {
                    await _appointmentService.CancelAppointmentAsync(id);
                    await Task.Run(() => LoadAppointments());
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