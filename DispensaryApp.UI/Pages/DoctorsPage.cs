using Gtk;
using System;
using System.Collections.Generic;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Dialogs;
using DispensaryApp.UI.Styles;

namespace DispensaryApp.UI.Pages
{
    public class DoctorsPage : Box
    {
        private readonly DoctorService _doctorService;
        private readonly TreeView _treeView;
        private readonly ListStore _store;
        private readonly Button _addButton;
        private readonly Button _editButton;
        private readonly Button _deleteButton;
        private readonly ScrolledWindow _scrolledWindow;

        public DoctorsPage() : base(Orientation.Vertical, 0)
        {
            _doctorService = new DoctorService();
            
            // Панель инструментов
            var hbox = new Box(Orientation.Horizontal, 6) { MarginStart = 6, MarginEnd = 6, MarginTop = 6, MarginBottom = 6 };
            
            _addButton = new Button("Добавить");
            _editButton = new Button("Редактировать");
            _deleteButton = new Button("Удалить");
            
            StyleManager.ApplyButtonStyle(_addButton);
            StyleManager.ApplyButtonStyle(_editButton);
            StyleManager.ApplyButtonStyle(_deleteButton);
            
            hbox.PackStart(_addButton, false, false, 0);
            hbox.PackStart(_editButton, false, false, 0);
            hbox.PackStart(_deleteButton, false, false, 0);
            hbox.PackEnd(new Label(""), true, true, 0);
            
            PackStart(hbox, false, false, 0);

            // Таблица врачей
            _store = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string), 
                                 typeof(string), typeof(string), typeof(string), typeof(string), 
                                 typeof(string), typeof(string));
            
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
            _treeView.AppendColumn("Специализация", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Лицензия", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Телефон", new CellRendererText(), "text", 6);
            _treeView.AppendColumn("Email", new CellRendererText(), "text", 7);
            _treeView.AppendColumn("График", new CellRendererText(), "text", 8);
            _treeView.AppendColumn("Дата найма", new CellRendererText(), "text", 9);
            
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
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            try
            {
                _store.Clear();
                var doctors = _doctorService.GetAllDoctors();
                
                foreach (var doctor in doctors)
                {
                    _store.AppendValues(
                        doctor.Id,
                        doctor.LastName,
                        doctor.FirstName,
                        doctor.MiddleName,
                        doctor.Specialization,
                        doctor.LicenseNumber,
                        doctor.Phone,
                        doctor.Email,
                        doctor.Schedule,
                        doctor.HireDate.ToString("dd.MM.yyyy")
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось загрузить список врачей: {ex.Message}", MessageType.Error);
            }
        }

        private void OnAddButtonClicked(object? sender, EventArgs e)
        {
            if (Toplevel is Window parent)
            {
                var dialog = new DoctorDialog(parent);
                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    LoadDoctors();
                }
                dialog.Destroy();
            }
        }

        private void OnEditButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                var doctor = _doctorService.GetDoctorById(id);
                
                if (doctor != null)
                {
                    if (Toplevel is Window parent)
                    {
                        var dialog = new DoctorDialog(parent, doctor);
                        if (dialog.Run() == (int)ResponseType.Accept)
                        {
                            LoadDoctors();
                        }
                        dialog.Destroy();
                    }
                }
            }
            else
            {
                ShowMessage("Предупреждение", "Выберите врача для редактирования", MessageType.Warning);
            }
        }

        private void OnDeleteButtonClicked(object? sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var id = (int)_store.GetValue(iter, 0);
                var doctor = _doctorService.GetDoctorById(id);
                
                if (doctor != null)
                {
                    var dialog = new MessageDialog(
                        Toplevel as Window,
                        DialogFlags.Modal,
                        MessageType.Question,
                        ButtonsType.YesNo,
                        "Вы уверены, что хотите удалить врача {0} {1} {2}?",
                        doctor.LastName,
                        doctor.FirstName,
                        doctor.MiddleName
                    );

                    if (dialog.Run() == (int)ResponseType.Yes)
                    {
                        try
                        {
                            _doctorService.DeleteDoctor(id);
                            LoadDoctors();
                            ShowMessage("Успех", "Врач успешно удален", MessageType.Info);
                        }
                        catch (Exception ex)
                        {
                            ShowMessage("Ошибка", $"Не удалось удалить врача: {ex.Message}", MessageType.Error);
                        }
                    }
                    dialog.Destroy();
                }
            }
            else
            {
                ShowMessage("Предупреждение", "Выберите врача для удаления", MessageType.Warning);
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