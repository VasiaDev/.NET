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
    public class ReportsPage : Box
    {
        private readonly ListStore _listStore;
        private readonly TreeView _treeView;
        private readonly ReportService _reportService;

        public ReportsPage(ReportService reportService) : base(Orientation.Vertical, 5)
        {
            _reportService = reportService;

            // Создаем модель данных
            _listStore = new ListStore(
                typeof(int),    // ID
                typeof(string), // Дата
                typeof(string), // Время
                typeof(string), // Пациент
                typeof(string), // Врач
                typeof(string), // Причина
                typeof(string)  // Статус
            );

            // Создаем представление
            _treeView = new TreeView(_listStore)
            {
                HeadersVisible = true,
                Reorderable = true
            };

            // Добавляем колонки
            _treeView.AppendColumn("ID", new CellRendererText(), "text", 0);
            _treeView.AppendColumn("Дата", new CellRendererText(), "text", 1);
            _treeView.AppendColumn("Время", new CellRendererText(), "text", 2);
            _treeView.AppendColumn("Пациент", new CellRendererText(), "text", 3);
            _treeView.AppendColumn("Врач", new CellRendererText(), "text", 4);
            _treeView.AppendColumn("Причина", new CellRendererText(), "text", 5);
            _treeView.AppendColumn("Статус", new CellRendererText(), "text", 6);

            // Создаем кнопки
            var buttonBox = new Box(Orientation.Horizontal, 5);
            var generateButton = new Button("Сгенерировать отчет");
            var exportButton = new Button("Экспорт");

            buttonBox.PackStart(generateButton, true, true, 5);
            buttonBox.PackStart(exportButton, true, true, 5);

            // Подключаем обработчики
            generateButton.Clicked += OnGenerateClicked;
            exportButton.Clicked += OnExportClicked;

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
                var appointments = await _reportService.GetAppointmentsAsync();
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

        private async void OnGenerateClicked(object? sender, EventArgs e)
        {
            var dialog = new ReportDialog(this.Toplevel as Window);
            if (dialog.Run() == (int)ResponseType.Accept)
            {
                await LoadDataAsync();
            }
            dialog.Destroy();
        }

        private async void OnExportClicked(object? sender, EventArgs e)
        {
            try
            {
                await _reportService.ExportToExcelAsync();
                var dialog = new MessageDialog(
                    this.Toplevel as Window,
                    DialogFlags.Modal,
                    MessageType.Info,
                    ButtonsType.Ok,
                    "Отчет успешно экспортирован"
                );
                dialog.Run();
                dialog.Destroy();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(
                    this.Toplevel as Window,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    $"Ошибка при экспорте отчета: {ex.Message}"
                );
                dialog.Run();
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