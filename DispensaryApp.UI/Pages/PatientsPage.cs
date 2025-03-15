using Gtk;
using System;
using System.Collections.Generic;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Dialogs;
using DispensaryApp.UI.Styles;

namespace DispensaryApp.UI.Pages
{
    public class PatientsPage : Box
    {
        private readonly PatientService _patientService;
        private readonly TreeView _treeView;
        private readonly ListStore _store;
        private readonly Button _addButton;
        private readonly Button _editButton;
        private readonly Button _deleteButton;
        private readonly ScrolledWindow _scrolledWindow;

        public PatientsPage() : base(Orientation.Vertical, 0)
        {
            _patientService = new PatientService();
            
            // Панель инструментов
            var toolbar = new Box(Orientation.Horizontal, 6) { MarginStart = 6, MarginEnd = 6, MarginTop = 6, MarginBottom = 6 };
            
            _addButton = new Button("Добавить");
            _editButton = new Button("Редактировать");
            _deleteButton = new Button("Удалить");
            
            StyleManager.ApplyButtonStyle(_addButton);
            StyleManager.ApplyButtonStyle(_editButton);
            StyleManager.ApplyButtonStyle(_deleteButton);
            
            toolbar.PackStart(_addButton, false, false, 0);
            toolbar.PackStart(_editButton, false, false, 0);
            toolbar.PackStart(_deleteButton, false, false, 0);
            toolbar.PackEnd(new Label(""), true, true, 0);
            
            PackStart(toolbar, false, false, 0);

            // Таблица пациентов
            _store = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string), 
                                 typeof(string), typeof(string), typeof(string), typeof(string));
            
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
            _treeView.AppendColumn("Фамилия", new CellRendererText(), "text", 1);
            _treeView.AppendColumn("Имя", new CellRendererText(), "text", 2);
            _treeView.AppendColumn("Отчество", new CellRendererText(), "text", 3);
            _treeView.AppendColumn("Полис", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Телефон", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Email", new CellRendererText(), "text", 6);
            _treeView.AppendColumn("Адрес", new CellRendererText(), "text", 7);
            
            _scrolledWindow = new ScrolledWindow
            {
                Child = _treeView,
                ShadowType = ShadowType.In
            };
            
            PackStart(_scrolledWindow, true, true, 0);

            // Подключаем обработчики событий
            _addButton.Clicked += OnAddButtonClicked;
            _editButton.Clicked += OnEditButtonClicked;
            _deleteButton.Clicked += OnDeleteButtonClicked;

            // Загружаем данные
            LoadPatients();
        }

        private void LoadPatients()
        {
            try
            {
                _store.Clear();
                var patients = _patientService.GetAllPatients();
                
                foreach (var patient in patients)
                {
                    _store.AppendValues(
                        patient.Id,
                        patient.LastName,
                        patient.FirstName,
                        patient.MiddleName,
                        patient.InsurancePolicy,
                        patient.Phone,
                        patient.Email,
                        patient.Address
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось загрузить список пациентов: {ex.Message}", MessageType.Error);
            }
        }

        private void OnAddButtonClicked(object? sender, EventArgs e)
        {
            if (Toplevel is Window parent)
            {
                var dialog = new PatientDialog(parent);
                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    LoadPatients();
                }
                dialog.Destroy();
            }
        }

        private void OnEditButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                var patient = _patientService.GetPatientById(id);
                
                if (patient != null)
                {
                    if (Toplevel is Window parent)
                    {
                        var dialog = new PatientDialog(parent, patient);
                        if (dialog.Run() == (int)ResponseType.Accept)
                        {
                            LoadPatients();
                        }
                        dialog.Destroy();
                    }
                }
            }
            else
            {
                ShowMessage("Предупреждение", "Выберите пациента для редактирования", MessageType.Warning);
            }
        }

        private void OnDeleteButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                var patient = _patientService.GetPatientById(id);
                
                if (patient != null)
                {
                    var dialog = new MessageDialog(
                        Toplevel as Window,
                        DialogFlags.Modal,
                        MessageType.Question,
                        ButtonsType.YesNo,
                        "Вы уверены, что хотите удалить пациента {0} {1} {2}?",
                        patient.LastName,
                        patient.FirstName,
                        patient.MiddleName
                    );

                    if (dialog.Run() == (int)ResponseType.Yes)
                    {
                        try
                        {
                            _patientService.DeletePatient(id);
                            LoadPatients();
                            ShowMessage("Успех", "Пациент успешно удален", MessageType.Info);
                        }
                        catch (Exception ex)
                        {
                            ShowMessage("Ошибка", $"Не удалось удалить пациента: {ex.Message}", MessageType.Error);
                        }
                    }
                    dialog.Destroy();
                }
            }
            else
            {
                ShowMessage("Предупреждение", "Выберите пациента для удаления", MessageType.Warning);
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