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
    public class DoctorsPage : Box
    {
        private readonly ListStore _listStore;
        private readonly TreeView _treeView;
        private readonly DoctorService _doctorService;

        public DoctorsPage(DoctorService doctorService) : base(Orientation.Vertical, 5)
        {
            _doctorService = doctorService;

            // Создаем модель данных
            _listStore = new ListStore(
                typeof(int),    // ID
                typeof(string), // Фамилия
                typeof(string), // Имя
                typeof(string), // Отчество
                typeof(string), // Специальность
                typeof(string), // Специализация
                typeof(string), // Номер лицензии
                typeof(string), // Телефон
                typeof(string)  // Email
            );

            // Создаем представление
            _treeView = new TreeView(_listStore)
            {
                HeadersVisible = true,
                Reorderable = true
            };

            // Добавляем колонки
            _treeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            _treeView.AppendColumn("Фамилия", new CellRendererText(), "text", 1);
            _treeView.AppendColumn("Имя", new CellRendererText(), "text", 2);
            _treeView.AppendColumn("Отчество", new CellRendererText(), "text", 3);
            _treeView.AppendColumn("Специальность", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Специализация", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Номер лицензии", new CellRendererText(), "text", 6);
            _treeView.AppendColumn("Телефон", new CellRendererText(), "text", 7);
            _treeView.AppendColumn("Email", new CellRendererText(), "text", 8);

            // Создаем кнопки
            var buttonBox = new Box(Orientation.Horizontal, 5);
            var addButton = new Button("Добавить");
            var editButton = new Button("Редактировать");
            var deleteButton = new Button("Удалить");

            buttonBox.PackStart(addButton, true, true, 5);
            buttonBox.PackStart(editButton, true, true, 5);
            buttonBox.PackStart(deleteButton, true, true, 5);

            // Подключаем обработчики
            addButton.Clicked += OnAddClicked;
            editButton.Clicked += OnEditClicked;
            deleteButton.Clicked += OnDeleteClicked;

            // Создаем скролл для таблицы
            var scrollWindow = new ScrolledWindow
            {
                ShadowType = ShadowType.In,
                HscrollbarPolicy = PolicyType.Automatic,
                VscrollbarPolicy = PolicyType.Automatic
            };
            scrollWindow.Add(_treeView);

            // Добавляем элементы на страницу
            PackStart(buttonBox, false, false, 5);
            PackStart(scrollWindow, true, true, 5);

            // Загружаем данные
            _ = LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                _listStore.Clear();
                var doctors = await _doctorService.GetAllAsync();
                foreach (var doctor in doctors)
                {
                    _listStore.AppendValues(
                        doctor.Id,
                        doctor.LastName,
                        doctor.FirstName,
                        doctor.MiddleName,
                        doctor.Specialty,
                        doctor.Specialization,
                        doctor.LicenseNumber,
                        doctor.Phone,
                        doctor.Email
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
            var dialog = new DoctorDialog(this.Toplevel as Window);
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
                var doctor = await _doctorService.GetByIdAsync(id);
                var dialog = new DoctorDialog(this.Toplevel as Window, doctor);
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
                var dialog = new MessageDialog(
                    this.Toplevel as Window,
                    DialogFlags.Modal,
                    MessageType.Question,
                    ButtonsType.YesNo,
                    "Вы уверены, что хотите удалить этого врача?"
                );
                if (dialog.Run() == (int)ResponseType.Yes)
                {
                    await _doctorService.DeleteAsync(id);
                    await LoadDataAsync();
                }
                dialog.Destroy();
            }
        }

        public new void Show()
        {
            base.Show();
            _ = LoadDataAsync();
        }
    }
} 