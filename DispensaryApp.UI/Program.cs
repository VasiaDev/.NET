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
using System.IO;

namespace DispensaryApp.UI
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Application.Init();
                StyleManager.Initialize();
                
                // Проверяем наличие файла конфигурации
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("Файл конфигурации appsettings.json не найден");
                }

                // Настройка конфигурации
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Настройка сервисов
                var services = new ServiceCollection();
                ConfigureServices(services, configuration);
                var serviceProvider = services.BuildServiceProvider();

                // Проверяем подключение к базе данных и создаем её, если не существует
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DispensaryDbContext>();
                    try
                    {
                        // Создаем базу данных и применяем миграции
                        dbContext.Database.EnsureCreated();
                        Console.WriteLine("База данных успешно создана или уже существует");
                        
                        // Проверяем подключение
                        dbContext.Database.OpenConnection();
                        Console.WriteLine("Подключение к базе данных успешно установлено");
                        dbContext.Database.CloseConnection();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Ошибка при работе с базой данных: {ex.Message}");
                    }
                }

                // Создание и запуск главного окна
                var window = new MainWindow();
                window.ShowAll();
                
                Application.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Показываем диалог с ошибкой
                var dialog = new MessageDialog(
                    null,
                    DialogFlags.Modal,
                    MessageType.Error,
                    ButtonsType.Ok,
                    $"Произошла ошибка при запуске приложения:\n{ex.Message}"
                );
                dialog.Run();
                dialog.Destroy();
                
                Environment.Exit(1);
            }
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Строка подключения к базе данных не найдена в конфигурации");
            }

            // Добавляем контекст базы данных
            services.AddDbContext<DispensaryDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()
                ));

            // Добавляем другие сервисы здесь
        }
    }
} 