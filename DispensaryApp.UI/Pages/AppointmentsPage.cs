using Gtk;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DispensaryApp.Core.Services;
using DispensaryApp.Data.Models;
using DispensaryApp.UI.Dialogs;
using DispensaryApp.UI.Styles;
using GLib;

namespace DispensaryApp.UI.Pages
{
    public class AppointmentsPage : Box
    {
        private readonly ListStore _listStore;
        private readonly TreeView _treeView;
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;
        private readonly DoctorService _doctorService;

        public AppointmentsPage(AppointmentService appointmentService, PatientService patientService, DoctorService doctorService) 
            : base(Orientation.Vertical, 5)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;

            _listStore = new ListStore(
                typeof(int),    
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string)  
            );

            _treeView = new TreeView(_listStore)
            {
                HeadersVisible = true,
                Reorderable = true
            };

            _treeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            _treeView.AppendColumn("Дата", new CellRendererText(), "text", 1);
            _treeView.AppendColumn("Время", new CellRendererText(), "text", 2);
            _treeView.AppendColumn("Пациент", new CellRendererText(), "text", 3);
            _treeView.AppendColumn("Врач", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Причина", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Статус", new CellRendererText(), "text", 6);

            var buttonBox = new Box(Orientation.Horizontal, 5);
            var addButton = new Button("Добавить");
            var editButton = new Button("Редактировать");
            var deleteButton = new Button("Удалить");
            var cancelButton = new Button("Отменить");

            buttonBox.PackStart(addButton, true, true, 5);
            buttonBox.PackStart(editButton, true, true, 5);
            buttonBox.PackStart(deleteButton, true, true, 5);
            buttonBox.PackStart(cancelButton, true, true, 5);

            addButton.Clicked += OnAddClicked;
            editButton.Clicked += OnEditClicked;
            deleteButton.Clicked += OnDeleteClicked;
            cancelButton.Clicked += OnCancelClicked;

            var scrollWindow = new ScrolledWindow
            {
                ShadowType = ShadowType.In,
                HscrollbarPolicy = PolicyType.Automatic,
                VscrollbarPolicy = PolicyType.Automatic
            };
            scrollWindow.Add(_treeView);

            PackStart(buttonBox, false, false, 5);
            PackStart(scrollWindow, true, true, 5);

            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _listStore.Clear();
                var appointments = await _appointmentService.GetAllAsync();
                foreach (var appointment in appointments)
                {
                    _listStore.AppendValues(
                        appointment.Id,
                        appointment.AppointmentDate.ToString("dd.MM.yyyy"),
                        appointment.AppointmentDate.ToString("HH:mm"),
                        $"{appointment.Patient.LastName} {appointment.Patient.FirstName}",
                        $"{appointment.Doctor.LastName} {appointment.Doctor.FirstName}",
                        appointment.Reason,
                        appointment.Status.ToString()
                    );
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(
                    this.Toplevel as Window,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    $"Ошибка при загрузке данных: {ex.Message}"
                );
                dialog.Run();
                dialog.Destroy();
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            var dialog = new AppointmentDialog(this.Toplevel as Window);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                await LoadDataAsync();
            }
            dialog.Destroy();
        }

        private async void OnEditClicked(object? sender, EventArgs e)
        {
            var selection = _treeView.Selection;
            if (selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_listStore.GetValue(iter, 0);
                var appointment = await _appointmentService.GetByIdAsync(id);
                var dialog = new AppointmentDialog(this.Toplevel as Window, appointment);
                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    await LoadDataAsync();
                }
                dialog.Destroy();
            }
        }

        private async void OnDeleteClicked(object? sender, EventArgs e)
        {
            var selection = _treeView.Selection;
            if (selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_listStore.GetValue(iter, 0);
                var confirmDialog = new MessageDialog(
                    this.Toplevel as Window,
                    DialogFlags.Modal,
                    MessageType.Question,
                    ButtonsType.YesNo,
                    "Вы уверены, что хотите удалить эту запись?"
                );

                if (confirmDialog.Run() == (int)ResponseType.Yes)
                {
                    await _appointmentService.DeleteAsync(id);
                    await LoadDataAsync();
                }

                confirmDialog.Destroy();
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            var selection = _treeView.Selection;
            if (selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_listStore.GetValue(iter, 0);
                var confirmDialog = new MessageDialog(
                    this.Toplevel as Window,
                    DialogFlags.Modal,
                    MessageType.Question,
                    ButtonsType.YesNo,
                    "Вы уверены, что хотите отменить этот прием?"
                );

                if (confirmDialog.Run() == (int)ResponseType.Yes)
                {
                    await _appointmentService.CancelAppointmentAsync(id);
                    await LoadDataAsync();
                }

                confirmDialog.Destroy();
            }
        }

        public new void Show()
        {
            base.Show();
            _ = LoadDataAsync();
        }
    }
} 