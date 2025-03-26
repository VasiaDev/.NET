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
using Microsoft.Extensions.Logging;
using DispensaryApp.Core.Services;

namespace DispensaryApp.UI
{
    public class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Application.Init();
                StyleManager.Initialize();
                
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("Файл конфигурации appsettings.json не найден");
                }

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                DispensaryDbContextFactory.Initialize(configuration);

                var services = new ServiceCollection();
                ConfigureServices(services, configuration);
                ServiceProvider = services.BuildServiceProvider();

                using (var scope = ServiceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DispensaryDbContext>();
                    try
                    {
                        Console.WriteLine("Применение миграций базы данных...");
                        dbContext.Database.Migrate();
                        Console.WriteLine("Миграции успешно применены");
                        
                        DbInitializer.Initialize(dbContext);
                        Console.WriteLine("Тестовые данные успешно добавлены");
                        
                        dbContext.Database.OpenConnection();
                        Console.WriteLine("Подключение к базе данных успешно установлено");
                        dbContext.Database.CloseConnection();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при работе с базой данных: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        throw;
                    }
                }

                var window = new MainWindow();
                window.ShowAll();
                
                Application.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
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

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            services.AddDbContext<DispensaryDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()
                ));

            services.AddScoped<PatientService>();
            services.AddScoped<DoctorService>();
            services.AddScoped<AppointmentService>();
            services.AddScoped<ReportService>();
        }
    }
} 