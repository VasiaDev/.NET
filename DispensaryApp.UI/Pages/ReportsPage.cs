using Gtk;
using System;
using System.Collections.Generic;
using DispensaryApp.Core.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using DispensaryApp.Data;

namespace DispensaryApp.UI.Pages
{
    public class ReportsPage : Box
    {
        private readonly ReportService _reportService;
        private readonly TreeView _treeView;
        private readonly ListStore _store;
        private readonly Button _generateButton;
        private readonly Button _exportButton;
        private readonly ComboBox _reportTypeComboBox;
        private readonly ScrolledWindow _scrolledWindow;

        public ReportsPage(DispensaryDbContext context) : base(Orientation.Vertical, 0)
        {
            _reportService = new ReportService(context);
            
            // Панель инструментов
            var toolbar = new Box(Orientation.Horizontal, 6) { MarginStart = 6, MarginEnd = 6, MarginTop = 6, MarginBottom = 6 };
            
            _reportTypeComboBox = new ComboBox(new string[] { "Приемы", "Пациенты", "Врачи", "Доход" });
            _generateButton = new Button("Сформировать");
            _exportButton = new Button("Экспорт");
            
            StyleManager.ApplyButtonStyle(_generateButton);
            StyleManager.ApplyButtonStyle(_exportButton);
            
            toolbar.PackStart(new Label("Тип отчета:"), false, false, 0);
            toolbar.PackStart(_reportTypeComboBox, false, false, 0);
            toolbar.PackStart(_generateButton, false, false, 0);
            toolbar.PackStart(_exportButton, false, false, 0);
            toolbar.PackEnd(new Label(""), true, true, 0);
            
            PackStart(toolbar, false, false, 0);

            // Таблица отчета
            _store = new ListStore(typeof(string), typeof(string));
            
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
            
            _treeView.AppendColumn("Параметр", new CellRendererText(), "text", 0);
            _treeView.AppendColumn("Значение", new CellRendererText(), "text", 1);
            
            _scrolledWindow = new ScrolledWindow
            {
                Child = _treeView,
                ShadowType = ShadowType.In
            };
            
            PackStart(_scrolledWindow, true, true, 0);

            // Подключаем обработчики событий
            _generateButton.Clicked += OnGenerateButtonClicked;
            _exportButton.Clicked += OnExportButtonClicked;

            // Устанавливаем начальное значение
            _reportTypeComboBox.Active = 0;
        }

        private void OnGenerateButtonClicked(object? sender, EventArgs e)
        {
            try
            {
                _store.Clear();
                string report = _reportTypeComboBox.Active switch
                {
                    0 => _reportService.GenerateAppointmentsReport(),
                    1 => _reportService.GeneratePatientsReport(),
                    2 => _reportService.GenerateDoctorsReport(),
                    3 => _reportService.GenerateIncomeReport(),
                    _ => throw new ArgumentException("Неизвестный тип отчета")
                };

                var lines = report.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        _store.AppendValues(parts[0].Trim(), parts[1].Trim());
                    }
                    else
                    {
                        _store.AppendValues(line.Trim(), "");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось сформировать отчет: {ex.Message}", MessageType.Error);
            }
        }

        private void OnExportButtonClicked(object? sender, EventArgs e)
        {
            try
            {
                var dialog = new FileChooserDialog(
                    "Сохранить отчет",
                    Toplevel as Window,
                    FileChooserAction.Save,
                    "Отмена", ResponseType.Cancel,
                    "Сохранить", ResponseType.Accept
                );

                dialog.Filter = new FileFilter { Name = "Текстовые файлы" };
                dialog.Filter.AddPattern("*.txt");

                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    var filename = dialog.Filename;
                    if (!filename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        filename += ".txt";
                    }

                    var report = _reportTypeComboBox.Active switch
                    {
                        0 => _reportService.GenerateAppointmentsReport(),
                        1 => _reportService.GeneratePatientsReport(),
                        2 => _reportService.GenerateDoctorsReport(),
                        3 => _reportService.GenerateIncomeReport(),
                        _ => throw new ArgumentException("Неизвестный тип отчета")
                    };

                    System.IO.File.WriteAllText(filename, report);
                    ShowMessage("Успех", "Отчет успешно сохранен", MessageType.Info);
                }
                dialog.Destroy();
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось экспортировать отчет: {ex.Message}", MessageType.Error);
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