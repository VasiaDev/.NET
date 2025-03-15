using Gtk;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DispensaryApp.Data;
using Microsoft.EntityFrameworkCore;
using DispensaryApp.UI.Styles;
using System.Security.Cryptography;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DispensaryApp.UI
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();
            StyleManager.Initialize();
            
            // Настройка конфигурации
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            // Настройка сервисов
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Создание и запуск главного окна
            var window = new MainWindow();
            window.ShowAll();
            
            Application.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Добавляем контекст базы данных
            services.AddDbContext<DispensaryDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0))
                ));

            // Добавляем другие сервисы здесь
        }
    }
} 