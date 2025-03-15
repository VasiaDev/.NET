using Gtk;
using System;
using DispensaryApp.UI.Pages;
using DispensaryApp.UI.Styles;

namespace DispensaryApp.UI
{
    public class MainWindow : Window
    {
        private readonly Notebook _notebook;
        private readonly AppointmentsPage _appointmentsPage;
        private readonly PatientsPage _patientsPage;
        private readonly DoctorsPage _doctorsPage;
        private readonly ReportsPage _reportsPage;

        public MainWindow() : base("Система управления поликлиникой")
        {
            // Настройка окна
            DefaultWidth = 1024;
            DefaultHeight = 768;
            WindowPosition = WindowPosition.Center;

            // Создаем контейнер с вкладками
            _notebook = new Notebook
            {
                Scrollable = true,
                ShowBorder = true,
                ShowTabs = true,
                TabPos = PositionType.Top
            };

            // Создаем страницы
            _appointmentsPage = new AppointmentsPage();
            _patientsPage = new PatientsPage();
            _doctorsPage = new DoctorsPage();
            _reportsPage = new ReportsPage();

            // Добавляем страницы во вкладки
            _notebook.AppendPage(_appointmentsPage, new Label("Приемы"));
            _notebook.AppendPage(_patientsPage, new Label("Пациенты"));
            _notebook.AppendPage(_doctorsPage, new Label("Врачи"));
            _notebook.AppendPage(_reportsPage, new Label("Отчеты"));

            // Добавляем контейнер в окно
            Add(_notebook);

            // Подключаем обработчик закрытия окна
            DeleteEvent += OnDeleteEvent;
        }

        private void OnDeleteEvent(object? sender, DeleteEventArgs args)
        {
            Application.Quit();
        }
    }
} 