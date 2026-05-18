using System.Data.Entity;
using Confectionery.Helpers;
using Confectionery.Models;

namespace Confectionery.Data
{
    public class ConfectioneryDbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            var admin = new User
            {
                Name = "Администратор",
                Email = "admin@confectionery.com",
                PasswordHash = PasswordHelper.Hash("Admin123"),
                Role = "Admin",
                Phone = "+375291234567"
            };
            context.Users.Add(admin);

            var categories = new[]
            {
                new Category { Name = "Торты" },
                new Category { Name = "Пирожные" },
                new Category { Name = "Печенье" },
                new Category { Name = "Конфеты" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var products = new[]
            {
                new Product
                {
                    Name = "Торт «Наполеон»",
                    Description = "Классический слоёный торт с заварным кремом",
                    Composition = "Слоёное тесто, масло, молоко, яйца, сахар, ваниль",
                    Weight = 1.5,
                    Price = 45.00m,
                    CategoryId = 1,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Торт «Медовик»",
                    Description = "Нежный медовый торт со сметанным кремом",
                    Composition = "Мёд, мука, масло, сметана, сахар, яйца",
                    Weight = 1.2,
                    Price = 38.00m,
                    CategoryId = 1,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Торт «Красный бархат»",
                    Description = "Эффектный торт с кремом чиз",
                    Composition = "Мука, кефир, краситель, сыр, масло, сахар",
                    Weight = 1.4,
                    Price = 52.00m,
                    CategoryId = 1,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Эклер классический",
                    Description = "Французское заварное пирожное с ванильным кремом",
                    Composition = "Заварное тесто, заварной крем, шоколадная глазурь",
                    Weight = 0.1,
                    Price = 5.50m,
                    CategoryId = 2,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Макарон ассорти",
                    Description = "Французские миндальные пирожные, набор 6 шт",
                    Composition = "Миндальная мука, меренга, различные начинки",
                    Weight = 0.09,
                    Price = 12.00m,
                    CategoryId = 2,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Тирамису",
                    Description = "Итальянский десерт с маскарпоне",
                    Composition = "Маскарпоне, савоярди, эспрессо, яйца, какао",
                    Weight = 0.25,
                    Price = 8.50m,
                    CategoryId = 2,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Печенье «Домашнее»",
                    Description = "Рассыпчатое сливочное печенье",
                    Composition = "Мука, масло, сахар, яйца, ваниль",
                    Weight = 0.5,
                    Price = 12.00m,
                    CategoryId = 3,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Шоколадные конфеты",
                    Description = "Ассорти трюфелей ручной работы, 12 шт",
                    Composition = "Тёмный шоколад, сливки, масло, различные начинки",
                    Weight = 0.2,
                    Price = 18.00m,
                    CategoryId = 4,
                    IsAvailable = true
                }
            };
            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
