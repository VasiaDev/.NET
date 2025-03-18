using Gtk;
using System;
using DispensaryApp.UI.Pages;
using DispensaryApp.UI.Styles;
using DispensaryApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DispensaryApp.UI
{
    public class MainWindow : Window
    {
        private readonly Notebook _notebook;
        private readonly AppointmentsPage _appointmentsPage;
        private readonly PatientsPage _patientsPage;
        private readonly DoctorsPage _doctorsPage;
        private readonly ReportsPage _reportsPage;
        private readonly DispensaryDbContext _dbContext;

        public MainWindow() : base("Система управления поликлиникой")
        {
            // Настройка окна
            DefaultWidth = 1024;
            DefaultHeight = 768;
            WindowPosition = WindowPosition.Center;

            // Инициализация контекста базы данных
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DispensaryDbContext>();
            optionsBuilder.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 21))
            );

            _dbContext = new DispensaryDbContext(optionsBuilder.Options);
            _dbContext.Database.EnsureCreated();

            // Создаем контейнер с вкладками
            _notebook = new Notebook
            {
                Scrollable = true,
                ShowBorder = true,
                ShowTabs = true,
                TabPos = PositionType.Top
            };

            // Создаем страницы
            _appointmentsPage = new AppointmentsPage(_dbContext);
            _patientsPage = new PatientsPage(_dbContext);
            _doctorsPage = new DoctorsPage(_dbContext);
            _reportsPage = new ReportsPage(_dbContext);

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

        private void OnDeleteEvent(object sender, DeleteEventArgs args)
        {
            _dbContext.Dispose();
            Application.Quit();
        }
    }
} 