using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace DispensaryApp.Data
{
    public static class DispensaryDbContextFactory
    {
        private static DbContextOptions<DispensaryDbContext> _options;

        public static void Initialize(IConfiguration configuration)
        {
            Console.WriteLine("Инициализация фабрики контекста базы данных...");
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Строка подключения к базе данных не найдена в конфигурации");
            }
            Console.WriteLine($"Строка подключения получена: {connectionString}");

            try
            {
                _options = new DbContextOptionsBuilder<DispensaryDbContext>()
                    .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)))
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(Console.WriteLine)
                    .Options;
                Console.WriteLine("Опции контекста базы данных настроены");

                // Проверяем подключение к базе данных
                using (var context = new DispensaryDbContext(_options))
                {
                    Console.WriteLine("Проверка подключения к базе данных...");
                    context.Database.OpenConnection();
                    Console.WriteLine("Подключение к базе данных успешно установлено");
                    context.Database.CloseConnection();

                    // Проверяем наличие таблиц
                    Console.WriteLine("Проверка наличия таблиц...");
                    var tables = context.Model.GetEntityTypes();
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"Найдена таблица: {table.GetTableName()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при инициализации базы данных: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static DispensaryDbContext CreateContext()
        {
            Console.WriteLine("Создание нового контекста базы данных...");
            if (_options == null)
            {
                throw new InvalidOperationException("Фабрика контекста базы данных не инициализирована. Вызовите метод Initialize перед созданием контекста.");
            }

            var context = new DispensaryDbContext(_options);
            Console.WriteLine("Новый контекст базы данных создан");
            return context;
        }
    }
} 