using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ConfectioneryShop.Models;

namespace ConfectioneryShop.Data
{
    public static class DataService
    {
        private static string _filePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Data_1",
            "products.xml"
        );

        public static List<Product> LoadProducts()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return GetSampleProducts();
                }

                XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
                using (FileStream fs = new FileStream(_filePath, FileMode.Open))
                {
                    return (List<Product>)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                return GetSampleProducts();
            }
        }

        public static void SaveProducts(List<Product> products)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
                using (FileStream fs = new FileStream(_filePath, FileMode.Create))
                {
                    serializer.Serialize(fs, products);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

  
        private static List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    Id = 1,
                    ShortName = "Макарон",
                    FullName = "Макарон с малиной",
                    Description = "Нежное французское пирожное с малиновым кремом. Хрустящая корочка снаружи и мягкая начинка внутри.",
                    ImagePath = "macaron.jpg",
                    Category = "Пирожные",
                    Rating = 4.8,
                    Price = 3.50m,      
                    Quantity = 50,
                    Color = "Розовый",
                    Size = "5 см",
                    Country = "Франция",
                    Discount = 10,
                    IsOutOfStock = false,
                    SoldCount = 150,
                    Manufacturer = "Французская кондитерская"
                },
                new Product
                {
                    Id = 2,
                    ShortName = "Торт Наполеон",
                    FullName = "Торт Наполеон классический",
                    Description = "Слоеный торт с нежным заварным кремом. Рецепт проверен годами. Вес 1 кг.",
                    ImagePath = "napoleon.jpg",
                    Category = "Торты",
                    Rating = 4.9,
                    Price = 28.00m,     
                    Quantity = 15,
                    Color = "Золотистый",
                    Size = "1 кг",
                    Country = "Беларусь",
                    Discount = 0,
                    IsOutOfStock = false,
                    SoldCount = 320,
                    Manufacturer = "Минская кондитерская фабрика"
                },
                new Product
                {
                    Id = 3,
                    ShortName = "Эклер",
                    FullName = "Эклер с шоколадным кремом",
                    Description = "Заварное пирожное с насыщенным шоколадным кремом и глазурью.",
                    ImagePath = "eclair.jpg",
                    Category = "Пирожные",
                    Rating = 4.7,
                    Price = 2.80m,      
                    Quantity = 30,
                    Color = "Коричневый",
                    Size = "10 см",
                    Country = "Беларусь",
                    Discount = 5,
                    IsOutOfStock = false,
                    SoldCount = 280,
                    Manufacturer = "Слодыч"
                },
                new Product
                {
                    Id = 4,
                    ShortName = "Кекс",
                    FullName = "Кекс с изюмом",
                    Description = "Классический кекс с изюмом и ванилью. Вкус из детства.",
                    ImagePath = "cake.jpg",
                    Category = "Кексы",
                    Rating = 4.5,
                    Price = 2.20m,     
                    Quantity = 0,
                    Color = "Бежевый",
                    Size = "100 г",
                    Country = "Беларусь",
                    Discount = 0,
                    IsOutOfStock = true,
                    SoldCount = 450,
                    Manufacturer = "Кондитерский комбинат"
                },
                new Product
                {
                    Id = 5,
                    ShortName = "Безе",
                    FullName = "Безе воздушное",
                    Description = "Легкое и хрустящее безе, тает во рту. Идеально к чаю.",
                    ImagePath = "meringue.jpg",
                    Category = "Пирожные",
                    Rating = 4.6,
                    Price = 1.50m,   
                    Quantity = 100,
                    Color = "Белый",
                    Size = "4 см",
                    Country = "Беларусь",
                    Discount = 0,
                    IsOutOfStock = false,
                    SoldCount = 620,
                    Manufacturer = "Домашняя кондитерская"
                },
                new Product
                {
                    Id = 6,
                    ShortName = "Торт Красный бархат",
                    FullName = "Торт Красный бархат с сырным кремом",
                    Description = "Бархатистый красный бисквит с нежнейшим сырным кремом. Вес 1.2 кг.",
                    ImagePath = "redvelvet.jpg",
                    Category = "Торты",
                    Rating = 5.0,
                    Price = 35.00m,     
                    Quantity = 8,
                    Color = "Красный",
                    Size = "1.2 кг",
                    Country = "Беларусь",
                    Discount = 15,
                    IsOutOfStock = false,
                    SoldCount = 95,
                    Manufacturer = "Минская кондитерская фабрика"
                }
            };
        }
    }
}