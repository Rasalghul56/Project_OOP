namespace Confectionery.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Confectionery.Helpers;
    using Confectionery.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Confectionery.Data.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Confectionery.Data.AppDbContext";
        }

        protected override void Seed(Confectionery.Data.AppDbContext ctx)
        {
            // ── Пользователи ──────────────────────────────────────────────────
            ctx.Users.AddOrUpdate(u => u.Email,
                new User { Name = "Администратор", Email = "admin@mail.ru", PasswordHash = PasswordHelper.Hash("admin"), Role = "Admin", Phone = "+375291234567" }
            );
            ctx.SaveChanges();

            // ── Категории ─────────────────────────────────────────────────────
            ctx.Categories.AddOrUpdate(c => c.Name,
                new Category { Name = "Торты"    },
                new Category { Name = "Пирожные" },
                new Category { Name = "Печенье"  },
                new Category { Name = "Конфеты"  },
                new Category { Name = "Зефир"    },
                new Category { Name = "Шоколад"  }
            );
            ctx.SaveChanges();

            // Получаем ID категорий по имени
            var cats = ctx.Categories.ToDictionary(c => c.Name, c => c.Id);

            // ── Товары ────────────────────────────────────────────────────────
            // ВАЖНО: добавляем товары только при первом запуске (если БД пуста).
            // Это гарантирует что ImagePath и другие поля, изменённые администратором,
            // никогда не перезаписываются при повторных запусках приложения.
            if (ctx.Products.Any()) return;

            ctx.Products.AddRange(new[]
            {
                // Торты
                new Product {
                    Name = "Торт «Наполеон»",
                    Description = "Классический многослойный торт с нежным заварным кремом. Тонкие хрустящие коржи пропитываются кремом в течение ночи — результат всегда безупречен.",
                    Composition = "Мука пшеничная, масло сливочное 82%, молоко цельное, яйца, сахар-рафинад, ванилин",
                    Weight = 1.5, Price = 45.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Медовик»",
                    Description = "Душистые медовые коржи и нежный сметанный крем — сочетание, проверенное временем. Торт выстаивается 12 часов для идеальной пропитки.",
                    Composition = "Мёд натуральный, мука, масло сливочное, сметана 25%, сахар, яйца, сода",
                    Weight = 1.2, Price = 38.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Красный бархат»",
                    Description = "Эффектный американский торт с бархатистым бисквитом и воздушным кремом из сыра. Идеален для особых случаев.",
                    Composition = "Мука, кефир, масло растительное, краситель пищевой, сливочный сыр, масло, сахарная пудра",
                    Weight = 1.4, Price = 52.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Прага»",
                    Description = "Советская классика с насыщенным шоколадным бисквитом, сливочно-масляным кремом и шоколадной глазурью.",
                    Composition = "Шоколад тёмный 72%, мука, яйца, масло, сметана, какао, сгущённое молоко",
                    Weight = 1.3, Price = 48.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Птичье молоко»",
                    Description = "Нежнейший суфле-торт на агар-агаре с тонкими бисквитными коржами и шоколадной глазурью. ГОСТ-рецептура.",
                    Composition = "Яичные белки, сахар, агар-агар, масло сливочное, сгущёнка, шоколад, бисквит",
                    Weight = 1.0, Price = 42.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Торт «Захер»",
                    Description = "Венская легенда: плотный шоколадный бисквит с абрикосовым джемом, покрытый тёмной ганашевой глазурью.",
                    Composition = "Шоколад 55%, масло, яйца, мука, сахар, джем абрикосовый, сливки",
                    Weight = 1.1, Price = 55.00m, CategoryId = cats["Торты"], IsAvailable = true
                },
                new Product {
                    Name = "Морковный торт",
                    Description = "Ароматный влажный бисквит с морковью, грецкими орехами и нежным кремом из маскарпоне с ноткой цитруса.",
                    Composition = "Морковь свежая, мука, масло растительное, яйца, сахар, орехи грецкие, маскарпоне, цедра апельсина",
                    Weight = 1.2, Price = 47.00m, CategoryId = cats["Торты"], IsAvailable = true
                },

                // Пирожные
                new Product {
                    Name = "Эклер классический",
                    Description = "Французское заварное пирожное с ванильным кремом «Дипломат» и зеркальной шоколадной глазурью. Изготавливается в день продажи.",
                    Composition = "Заварное тесто, масло, яйца, крем патисьер, шоколад, сливки 33%",
                    Weight = 0.10, Price = 5.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Макарон ассорти (6 шт.)",
                    Description = "Шесть французских миндальных пирожных с разными начинками: малина, ваниль, фисташка, шоколад, лимон, карамель.",
                    Composition = "Миндальная мука, яичный белок, сахарная пудра, пищевые красители, ганаш, конфитюр",
                    Weight = 0.09, Price = 12.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Тирамису",
                    Description = "Итальянский десерт в исполнении нашего кондитера: печенье савоярди, пропитанное крепким эспрессо, с воздушным кремом из маскарпоне.",
                    Composition = "Маскарпоне, яйца, савоярди, эспрессо, сахар, какао Barry",
                    Weight = 0.25, Price = 8.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Чизкейк «Нью-Йорк»",
                    Description = "Нежный сливочный чизкейк на хрустящей основе из крекеров. Классический рецепт без лишних добавок — только сыр, сливки и ваниль.",
                    Composition = "Сыр сливочный, сливки 33%, яйца, сахар, крекеры, масло, ваниль",
                    Weight = 0.30, Price = 9.00m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Профитроли с кремом",
                    Description = "Нежные заварные шарики, наполненные кремом шантийи и покрытые шоколадным соусом. Порция — 5 штук.",
                    Composition = "Заварное тесто, сливки 35%, сахарная пудра, ваниль, шоколад",
                    Weight = 0.15, Price = 7.00m, CategoryId = cats["Пирожные"], IsAvailable = true
                },
                new Product {
                    Name = "Пирожное «Картошка»",
                    Description = "Классическое советское пирожное из бисквитной крошки со сливочным кремом, в виде небольшой «картошки».",
                    Composition = "Бисквит, масло сливочное, сгущёнка, какао, коньяк, вафельная крошка",
                    Weight = 0.12, Price = 4.50m, CategoryId = cats["Пирожные"], IsAvailable = true
                },

                // Печенье
                new Product {
                    Name = "Печенье «Домашнее»",
                    Description = "Рассыпчатое сливочное печенье по старинному рецепту. Каждое изделие лепится вручную. Упаковка 500 г.",
                    Composition = "Мука высший сорт, масло сливочное 82%, сахарная пудра, яйца, ваниль",
                    Weight = 0.50, Price = 12.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },
                new Product {
                    Name = "Овсяное печенье с изюмом",
                    Description = "Полезное и вкусное овсяное печенье с крупным изюмом. Выпекается без добавления разрыхлителей. Упаковка 400 г.",
                    Composition = "Овсяные хлопья, мука, масло, сахар тростниковый, яйца, изюм султана, корица",
                    Weight = 0.40, Price = 10.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },
                new Product {
                    Name = "Имбирное печенье (набор 12 шт.)",
                    Description = "Ароматное имбирное печенье в форме снежинок и сердечек, украшенное сахарной глазурью вручную.",
                    Composition = "Мука, масло, сахар, мёд, имбирь молотый, корица, гвоздика, яйца, сахарная глазурь",
                    Weight = 0.30, Price = 14.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },
                new Product {
                    Name = "Брауни шоколадный",
                    Description = "Плотный влажный американский брауни с хрустящей корочкой и тянущейся серединой. Порция 2 штуки.",
                    Composition = "Шоколад 70%, масло сливочное, сахар, яйца, мука, орехи пекан",
                    Weight = 0.18, Price = 8.00m, CategoryId = cats["Печенье"], IsAvailable = true
                },

                // Конфеты
                new Product {
                    Name = "Трюфели из бельгийского шоколада (12 шт.)",
                    Description = "Ручные трюфели из шоколада Callebaut: ганаш тёмный, молочный, белый — по 4 штуки каждого. В элегантной коробке.",
                    Composition = "Шоколад Callebaut 54%, сливки 35%, масло, какао, сахар",
                    Weight = 0.20, Price = 18.00m, CategoryId = cats["Конфеты"], IsAvailable = true
                },
                new Product {
                    Name = "Конфеты «Пралине» (10 шт.)",
                    Description = "Шоколадные конфеты с начинкой из фундукового пралине. Каждая конфета ручной лепки, украшена орехом.",
                    Composition = "Шоколад молочный, фундук, сахар, сливки, масло какао",
                    Weight = 0.15, Price = 15.00m, CategoryId = cats["Конфеты"], IsAvailable = true
                },
                new Product {
                    Name = "Мармелад ягодный (200 г)",
                    Description = "Натуральный мармелад из свежих ягод без красителей и консервантов. Малина, черника, клубника.",
                    Composition = "Ягодное пюре, сахар, пектин яблочный, лимонный сок",
                    Weight = 0.20, Price = 9.50m, CategoryId = cats["Конфеты"], IsAvailable = true
                },

                // Зефир
                new Product {
                    Name = "Зефир ванильный (6 шт.)",
                    Description = "Воздушный домашний зефир из яблочного пюре. Нежная текстура, натуральный аромат ванили. Без искусственных красителей.",
                    Composition = "Яблочное пюре, яичный белок, сахар, агар-агар, ваниль",
                    Weight = 0.24, Price = 8.50m, CategoryId = cats["Зефир"], IsAvailable = true
                },
                new Product {
                    Name = "Зефир клубничный (6 шт.)",
                    Description = "Розовый зефир с натуральным клубничным пюре. Нежный, тающий, с ярким ягодным вкусом.",
                    Composition = "Клубничное пюре, яичный белок, сахар, агар-агар",
                    Weight = 0.24, Price = 9.00m, CategoryId = cats["Зефир"], IsAvailable = true
                },
                new Product {
                    Name = "Зефир в шоколаде (8 шт.)",
                    Description = "Классический ванильный зефир, покрытый тонким слоем тёмного бельгийского шоколада. Контраст вкусов и текстур.",
                    Composition = "Яблочное пюре, белок, агар, шоколад тёмный Callebaut, сахар",
                    Weight = 0.28, Price = 11.00m, CategoryId = cats["Зефир"], IsAvailable = true
                },

                // Шоколад
                new Product {
                    Name = "Плитка «Горький шоколад 72%»",
                    Description = "Авторская плитка из какао-бобов Tanzania с нотами сухофруктов и терпкостью. Для истинных ценителей тёмного шоколада.",
                    Composition = "Какао-масса, какао-масло, сахар. Содержание какао ≥ 72%",
                    Weight = 0.10, Price = 7.50m, CategoryId = cats["Шоколад"], IsAvailable = true
                },
                new Product {
                    Name = "Плитка «Молочный шоколад с фундуком»",
                    Description = "Нежный молочный шоколад с крупными кусочками поджаренного фундука. Идеально к кофе или чаю.",
                    Composition = "Какао-масло, молоко сухое цельное, сахар, фундук, лецитин",
                    Weight = 0.10, Price = 6.50m, CategoryId = cats["Шоколад"], IsAvailable = true
                }
            });
            ctx.SaveChanges();

        }
    }
}
