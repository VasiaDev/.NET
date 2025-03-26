using Gtk;
using System;
using DispensaryApp.Data.Models;
using DispensaryApp.Core.Services;
using DispensaryApp.UI.Styles;
using Microsoft.Extensions.Logging;

namespace DispensaryApp.UI.Dialogs
{
    public class ReportDialog : Dialog
    {
        private readonly Entry _startDateEntry;
        private readonly Entry _endDateEntry;
        private readonly ComboBox _reportTypeComboBox;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public ReportDialog(Window parent) : base("Генерация отчета", parent, DialogFlags.Modal)
        {
            var vbox = new Box(Orientation.Vertical, 6) { BorderWidth = 12 };

            // Период отчета
            var dateBox = new Box(Orientation.Horizontal, 6);
            _startDateEntry = new Entry { PlaceholderText = "Дата начала (дд.мм.гггг)" };
            _endDateEntry = new Entry { PlaceholderText = "Дата окончания (дд.мм.гггг)" };
            dateBox.PackStart(_startDateEntry, true, true, 0);
            dateBox.PackStart(_endDateEntry, true, true, 0);
            vbox.PackStart(dateBox, false, false, 0);

            // Тип отчета
            var typeBox = new Box(Orientation.Horizontal, 6);
            typeBox.PackStart(new Label("Тип отчета:"), false, false, 0);
            _reportTypeComboBox = new ComboBox();
            var store = new ListStore(typeof(string));
            store.AppendValues("Общий отчет");
            store.AppendValues("Отчет по врачам");
            store.AppendValues("Отчет по пациентам");
            _reportTypeComboBox.Model = store;
            var renderer = new CellRendererText();
            _reportTypeComboBox.PackStart(renderer, true);
            _reportTypeComboBox.AddAttribute(renderer, "text", 0);
            _reportTypeComboBox.Active = 0;
            typeBox.PackStart(_reportTypeComboBox, true, true, 0);
            vbox.PackStart(typeBox, false, false, 0);

            // Кнопки
            var buttonBox = new Box(Orientation.Horizontal, 6);
            _saveButton = new Button("Сгенерировать");
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
            ShowAll();
        }

        private void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                if (!DateTime.TryParseExact(_startDateEntry.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                {
                    ShowMessage("Ошибка", "Неверный формат даты начала", MessageType.Error);
                    return;
                }

                if (!DateTime.TryParseExact(_endDateEntry.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    ShowMessage("Ошибка", "Неверный формат даты окончания", MessageType.Error);
                    return;
                }

                if (endDate < startDate)
                {
                    ShowMessage("Ошибка", "Дата окончания не может быть раньше даты начала", MessageType.Error);
                    return;
                }

                Respond(ResponseType.Accept);
            }
            catch (Exception ex)
            {
                ShowMessage("Ошибка", $"Не удалось сгенерировать отчет: {ex.Message}", MessageType.Error);
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